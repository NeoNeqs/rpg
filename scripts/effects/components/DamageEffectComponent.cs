using RPG.global;
using Godot;
using RPG.scripts.combat;

namespace RPG.scripts.effects.components;

[Tool, GlobalClass] 
public partial class DamageEffectComponent : EffectComponent {
    [Export] public DamageType DamageType;

    [Export] public long FlatValue;
    
    [Export] public Stats.IntegerStat StatScale;
    [Export] public float Coefficient;
    [Export] public short MaxStacks = 1;
    [Export] public Texture2D Icon;

    public float GetTotalDamage(Stats pAttackerStats) {
        return FlatValue + (pAttackerStats.GetIntegerStat(StatScale) * Coefficient);
    }
    
}