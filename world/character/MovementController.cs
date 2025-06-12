using Godot;

namespace RPG.world.character;

[GlobalClass]
public partial class MovementController : Node {
    private const float Speed = 5.0f;
    private const float JumpVelocity = 4.5f;

    private PlayerCharacterBody _body = null!;
    private Node3D _model = null!;
    private Node3D _pivot = null!;

    public override void _Ready() {
        _body = GetBody();
        _model = _body.GetModel();
    }

    public override void _PhysicsProcess(double pDelta) {
        Vector3 newVelocity = _body.Velocity;
        
        if (!_body.IsOnFloor()) {
            newVelocity += _body.GetGravity() * (float)pDelta;
        }

        if (Input.IsActionJustPressed("ui_accept") && _body.IsOnFloor()) {
            newVelocity.Y = JumpVelocity;
        }

        Vector2 inputDir = Input.GetVector("strafe_left", "strafe_right", "forward", "backwards");
        Vector3 direction = (_model.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero) {
            newVelocity.X = direction.X * Speed;
            newVelocity.Z = direction.Z * Speed;
        } else {
            newVelocity.X = Mathf.MoveToward(_body.Velocity.X, 0, Speed);
            newVelocity.Z = Mathf.MoveToward(_body.Velocity.Z, 0, Speed);
        }
        
        _body.Velocity = newVelocity;
        _body.MoveAndSlide();
    }

    private PlayerCharacterBody GetBody() {
        return GetParent<PlayerCharacterBody>();
    }
}
