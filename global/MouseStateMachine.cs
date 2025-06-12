using Godot;

namespace RPG.global;

public partial class MouseStateMachine : Node {

    // This is not null if this class is autoloaded by Godot as Singleton
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public static MouseStateMachine Instance;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    
    public enum State {
        Free,
        UIControl,
        InventoryControl,
        CameraControl,
        WorldInteract,
    }

    public MouseStateMachine() {
        Instance = this;
    }
    
    public State CurrentState { get; private set; }
    
    public Vector2 LastMousePosition = Vector2.Inf;

    public bool RequestStateDeferred(State pState) {
        return CallDeferred(nameof(RequestState), Variant.From(pState)).AsBool();
    }
    public bool RequestState(State pState) {
        bool shouldWarp = DisplayServer.MouseGetMode() is DisplayServer.MouseMode.Captured;
        
        switch (pState) {
            case State.Free:
                DisplayServer.MouseSetMode(DisplayServer.MouseMode.Visible);
                
                break;
            case State.UIControl:
                return false;
            case State.InventoryControl:
                if (CurrentState is not State.Free) {
                    return false;
                }

                DisplayServer.MouseSetMode(DisplayServer.MouseMode.Hidden);
                
                break;
            case State.CameraControl:
                if (CurrentState is not State.Free) {
                    return false;
                }
                
                LastMousePosition = GetViewport().GetMousePosition();
                DisplayServer.MouseSetMode(DisplayServer.MouseMode.Captured);
                
                break;
            case State.WorldInteract:
                DisplayServer.MouseSetMode(DisplayServer.MouseMode.Visible);
                
                break;
            default:
                Logger.Core.Error($"Unhandled state: {pState.ToString()}", true);
                
                return false;
        }
        
        CurrentState = pState;
        
        if (shouldWarp && LastMousePosition != Vector2.Inf) {
            GetViewport().WarpMouse(LastMousePosition);
        }
        
        return true;
    }
}