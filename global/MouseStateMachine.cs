using Godot;
using RPG.Global;

namespace RPG.global;

public static class MouseStateMachine {
    public enum State {
        Free,
        UIControl,
        CameraControl,
        WorldInteract,
    }

    private static State _currentState;

    public static void SetState(State pState) {
        switch (pState) {
            case State.Free:
                _currentState = pState;
                DisplayServer.MouseSetMode(DisplayServer.MouseMode.Visible);
                break;
            case State.UIControl:
                if (_currentState != State.Free) {
                    return;
                }
                
                _currentState = pState;
                DisplayServer.MouseSetMode(DisplayServer.MouseMode.Visible);
                break;
            case State.CameraControl:
                if (_currentState != State.Free) {
                    return;
                }
                
                _currentState = pState;
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