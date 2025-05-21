using Godot;
using RPG.scripts.combat;

namespace RPG.scripts.effects.components;

public partial class StatEffectComponent : EffectComponent {
    [Export] public Stats Stats = new();
}