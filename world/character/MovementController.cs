using Godot;

namespace RPG.world.character;

public partial class MovementController : Node {
    private const float Speed = 5.0f;


    private PlayerCharacterBody _body;
    private Node3D _model;

    public override void _Ready() {
        _body = GetBody();
        _model = _body.GetModel();
    }

    public override void _PhysicsProcess(double delta) {
        if (!_body.IsOnFloor()) {
            _body.Velocity += _body.GetGravity() * (float)delta;
        }        
    }

    private PlayerCharacterBody GetBody() {
        return GetParent<PlayerCharacterBody>();
    }
}