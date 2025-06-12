using Godot;

namespace RPG.world.character;

public partial class PlayerCharacterBody : CharacterBody3D {

    public Node3D GetModel() {
        return GetNodeOrNull<Node3D>("Model");
    }

    public Node3D GetPivot() {
        return GetNodeOrNull<Node3D>("Pivot");
    }
}