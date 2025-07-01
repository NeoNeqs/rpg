using global::RPG.global;
using Godot;

namespace RPG.world.character;

public partial class AoeDecal : Decal {
    [Export] private PlayerCharacter _character = null!;

    private RayCast3D RayCast => GetChild<RayCast3D>(0);
    private float _range;

    public override void _Ready() {
        Visible = false;
        SetPhysicsProcess(false);
    }

    public void Update(Vector3 pPosition, float pRange = 0.0f) {
        if (pPosition == Vector3.Inf) {
            Visible = false;
        } else {
            // IMPORTANT: Set GlobalPosition before making it visible to avoid ugly jumping around.
            GlobalPosition = pPosition;
            Visible = true;
            _range = pRange;
        }

        SetPhysicsProcess(Visible);
    }

    public override void _PhysicsProcess(double pDelta) {
        Vector3 playerPosition = _character.GetBody().GlobalPosition;
        Vector3 toTarget = _character.GetWorld().GetMouseWorldPosition(1_000_000) - playerPosition;
        if (toTarget.LengthSquared() > _range * _range) {
            toTarget = toTarget.Normalized() * _range;
            Vector3 clamped = playerPosition + toTarget;
            clamped.Y = RayCast.GetCollisionPoint().Y;
            GlobalPosition = clamped;
        } else {
            Vector3 clamped = playerPosition + toTarget;
            GlobalPosition = clamped;
        }
    }
}