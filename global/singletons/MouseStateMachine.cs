using Godot;
using RPG.ui.elements;
using RPG.ui.views.inventory;

namespace RPG.global.singletons;

/// <summary>
///     Manages mouse behavior (MouseMode and Position) based on an active state.
/// </summary>
public sealed partial class MouseStateMachine : Node {
    public enum State {
        /// Free state allows any other state to be activated.
        Free,

        /// Used when the player interacts with
        /// <see cref="UIElement" />
        /// s.
        UIControl,

        /// Used specifically when player interacts with
        /// <see cref="InventoryView" />
        /// s.
        InventoryControl,

        /// Used when player controls the camera.
        CameraControl,

        /// Used when player interacts with the world / environment, e.g. selecting entities,
        WorldInteract,

        Debug
    }
    // This is not null if this class is autoloaded by Godot as Singleton
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public static MouseStateMachine Instance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public MouseStateMachine() {
        // Prevent overwriting the Instance in case this singleton is instanced more than once
        Instance = this;
    }

    public State CurrentState { private set; get; } = State.Free;

    public Vector2 LastMousePosition { private set; get; } = Vector2.Inf;

    /// <summary>
    ///     Same as <see cref="RequestState" />, but the execution is delayed to the next frame.
    /// </summary>
    /// <param name="pNewState">Specifies the state this class should change to.</param>
    /// <returns><c>True</c> if the state was changed, otherwise <c>false</c>.</returns>
    public bool RequestStateDeferred(State pNewState) {
        return CallDeferred(nameof(RequestState), Variant.From(pNewState)).AsBool();
    }

    /// <summary>
    ///     <para>Requests the State Machine to change the current state to <see cref="pNewState" />.</para>
    ///     <para>
    ///         One cannot change the state to a non <see cref="State.Free" /> state if current state is NOT
    ///         <see cref="State.Free" />.
    ///     </para>
    ///     <para>
    ///         Paraphrasing: if the StateMachine is "busy" (not free) you cannot disturb it with another state with an
    ///         exception*.
    ///     </para>
    ///     <para>*The exception: At any point you can change the state to <see cref="State.Free" />.</para>
    ///     <para>
    ///         This will work:
    ///         <code>
    ///     // Notice the order of the states:
    ///     MouseStateMachine.Instance.RequestState(MouseStateMachine.State.UIControl);
    ///     MouseStateMachine.Instance.RequestState(MouseStateMachine.State.Free);
    ///     MouseStateMachine.Instance.RequestState(MouseStateMachine.State.CameraControl);
    ///     </code>
    ///     </para>
    ///     <para>
    ///         This will NOT as work:
    ///         <code>
    ///     // Notice the order of the states:
    ///     MouseStateMachine.Instance.RequestState(MouseStateMachine.State.UIControl);
    ///     // This following call will not change the state because the current state is State.UIControl and not State.Free
    ///     MouseStateMachine.Instance.RequestState(MouseStateMachine.State.CameraControl);
    ///     MouseStateMachine.Instance.RequestState(MouseStateMachine.State.Free);
    ///     </code>
    ///     </para>
    /// </summary>
    /// <param name="pNewState">Specifies the state this class should change to.</param>
    /// <returns><c>True</c> if the state was changed, otherwise <c>false</c>.</returns>
    public bool RequestState(State pNewState) {
        if (CurrentState is not State.Free && pNewState is not State.Free) {
            return false;
        }

        switch (pNewState) {
            case State.Free:
            case State.WorldInteract:
            case State.Debug:
            case State.UIControl:
                ShowMouse();

                break;
            case State.InventoryControl:
                DisplayServer.MouseSetMode(DisplayServer.MouseMode.Hidden);

                break;
            case State.CameraControl:
                CaptureMouse();

                break;
            default:
                Logger.Core.Error($"Unhandled mouse state found: {pNewState.ToString()}", true);

                return false;
        }

        CurrentState = pNewState;

        return true;
    }

    public static bool IsCaptured() {
        return DisplayServer.MouseGetMode() == DisplayServer.MouseMode.Captured;
    }

    private void ShowMouse() {
        // Prevent mouse flicker
        if (DisplayServer.MouseGetMode() == DisplayServer.MouseMode.Visible) {
            return;
        }

        bool shouldWarp = DisplayServer.MouseGetMode() is DisplayServer.MouseMode.Captured &&
                          LastMousePosition != Vector2.Inf;

        // Warping mouse when it's Hidden prevents weird glitching.
        DisplayServer.MouseSetMode(DisplayServer.MouseMode.Hidden);

        if (shouldWarp) {
            GetViewport().WarpMouse(LastMousePosition);
        }

        DisplayServer.MouseSetMode(DisplayServer.MouseMode.Visible);
    }

    private void CaptureMouse() {
        // IMPORTANT: Mouse position must be saved before changing the mode to Capture.
        //            Capture mode warps the mouse to the center of the window!
        LastMousePosition = GetViewport().GetMousePosition();
        DisplayServer.MouseSetMode(DisplayServer.MouseMode.Captured);
    }
}