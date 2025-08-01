using Godot;
using RPG.global.singletons;

namespace RPG.world.entity;

[GlobalClass]
public partial class RigidBody : RigidBody3D {
    [Signal]
    public delegate void BodySelectedEventHandler();

    private bool _isHovering;

    public override void _EnterTree() {
        MouseEntered += () => { _isHovering = true; };
        MouseExited += () => { _isHovering = false; };
    }

    public override void _UnhandledInput(InputEvent pEvent) {
        if (pEvent is InputEventMouseButton mouseButtonEvent && pEvent.IsPressed() &&
            mouseButtonEvent.ButtonIndex == MouseButton.Left && _isHovering) {
            if (MouseStateMachine.Instance.RequestState(MouseStateMachine.State.WorldInteract)) {
                EmitSignalBodySelected();
                GetViewport().SetInputAsHandled();
                MouseStateMachine.Instance.RequestState(MouseStateMachine.State.Free);
            }
        }
    }
}