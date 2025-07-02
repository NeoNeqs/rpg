using Godot;
using RPG.global.enums;
using RPG.scripts.combat;

namespace RPG.scripts.effects;

[Tool, GlobalClass]
public sealed partial class DamageEffect : StackingEffect {
    [Export] public DamageType DamageType { private set; get; }
    
    
    public float GetTotalDamage(Stats pAttackerStats) {
        return FlatValue + (pAttackerStats.GetIntegerStat(StatScale) * StatScaleCoefficient);
    }
}