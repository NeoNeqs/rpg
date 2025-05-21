using RPG.global;
using Godot;

namespace RPG.scripts.inventory.components;

[Tool, GlobalClass]
public partial class DamageComponent : GizmoComponent {
    [Export] public DamageType DamageType;

    [Export] public float Damage;
}