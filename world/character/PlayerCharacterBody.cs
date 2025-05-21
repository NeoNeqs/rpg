using Godot;

namespace RPG.world.character;

public partial class PlayerCharacterBody : CharacterBody3D {
    [Signal]
    public delegate void MouseClickedEventHandler(Vector3 pMousePosition);

    public override void _InputEvent(Camera3D pCam, InputEvent pEvent, Vector3 pPosition, Vector3 pNormal, int pIdx) {
        if (pEvent is not InputEventMouseButton mouseButton) return;
        if (mouseButton.IsPressed()) {
            EmitSignalMouseClicked(pPosition);
        }
    }
}