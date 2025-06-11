using Godot;
using RPG.scripts.combat;

namespace RPG.scripts.effects.components;

[Tool, GlobalClass]
public partial class StatEffectComponent : EffectComponent {
    [Export] public Stats Stats = new();
}