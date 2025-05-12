using Godot;
using RPG.Global;

namespace RPG.global;

public partial class MouseStateMachine : Node {

    public enum State {
        Free,
        UIControl,
        CameraControl,
        WorldControl,
    }
    
    private State _currentState;

    public void SetState(State pState) {
        if (_currentState != State.Free) {
            return;
        }
        
        _currentState = pState;
        _HandleState(_currentState);
    }

    private static void _HandleState(State pState) {
        switch (pState) {
            case State.Free:
                DisplayServer.MouseSetMode(DisplayServer.MouseMode.Visible);
                break;
            case State.UIControl:
                break;
            case State.CameraControl:
                DisplayServer.MouseSetMode(DisplayServer.MouseMode.Captured);
                break;
            case State.WorldControl:
                break;
            default:
                Logger.Core.Error($"Unhandled state: {pState.ToString()}", true);
                break;
        }
    }
}