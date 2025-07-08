using Godot;
using RPG.global.enums;

namespace RPG.scripts.inventory.components;

[Tool, GlobalClass]
public partial class DamageComponent : GizmoComponent {
    [Export] public DamageType DamageType;

    [Export] public float Damage;
}