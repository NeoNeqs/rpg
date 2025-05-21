using Godot;
using RPG.global;

namespace RPG.global;

public static class MouseStateMachine {
    public enum State {
        UIControl,
        InventoryControl,
        CameraControl,
        WorldInteract,
    }
    
    public static State CurrentState { get; private set; }

    public static void SetState(State pState) {
        switch (pState) {
            case State.UIControl:
                CurrentState = pState;
                DisplayServer.MouseSetMode(DisplayServer.MouseMode.Visible);
                break;
            case State.InventoryControl:
                DisplayServer.MouseSetMode(DisplayServer.MouseMode.Hidden);
                break;
            case State.CameraControl:
                CurrentState = pState;
                DisplayServer.MouseSetMode(DisplayServer.MouseMode.Captured);
                break;
            case State.WorldInteract:
                break;
            default:
                Logger.Core.Error($"Unhandled state: {pState.ToString()}", true);
                break;
        }
    }
}