using Godot;
using RPG.scripts.combat;

namespace RPG.scripts.inventory.components;

[Tool, GlobalClass]
public partial class StatComponent : GizmoComponent {
    [Export] public required Stats Stats;
}