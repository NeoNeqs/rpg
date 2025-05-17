using Godot;
using RPG.scripts.combat;

namespace RPG.scripts.components;

public partial class AttributeComponent : GizmoComponent {
    [Export] public required Attributes Attributes;
}