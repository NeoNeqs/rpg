using Godot;
using RPG.scripts.combat;

namespace RPG.scripts.inventory.components;

public partial class AttributeComponent : GizmoComponent {
    [Export] public required Attributes Attributes;
}