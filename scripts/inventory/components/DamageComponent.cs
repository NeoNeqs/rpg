using Godot;
using RPG.global.enums;

namespace RPG.scripts.inventory.components;

[Tool, GlobalClass]
public partial class DamageComponent : GizmoComponent {
    [Export] public float Damage;
    [Export] public DamageType DamageType;
}