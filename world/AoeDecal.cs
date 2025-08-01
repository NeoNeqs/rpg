using System.Threading.Tasks;
using Godot;
using RPG.world.character;

namespace RPG.world;

public partial class AoeDecal : Sprite3D {
    [Export] private PlayerCharacter _character = null!;

    private float _range;

    public override void _Ready() {
        Disable();
    }

    public async Task Update(float pRange) {
        _range = pRange;

        SetPhysicsProcess(true);
        // Wait for 1 physics frame, otherwise the decal will suddenly jump when set to visible
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        Visible = true;
    }

    public void Disable() {
        Visible = false;
        SetPhysicsProcess(false);
    }

    public override void _PhysicsProcess(double pDelta) {
        Vector3 playerPosition = _character.GetBody().GlobalPosition;
        Vector3 pos = _character.GetWorld().GetMouseWorldPosition(playerPosition, _range, 0b11);
        pos.Y += 0.001f;

        GlobalPosition = pos;
    }
}