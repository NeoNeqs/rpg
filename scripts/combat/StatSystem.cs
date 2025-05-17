using System;
using Godot;
using RPG.Global;
using RPG.scripts.effects;

namespace RPG.scripts.combat;

public partial class StatSystem : Resource {

    [Export] public Attributes Attributes;
    [Export] public Stats Stats;
    
    public void Initialize(Attributes pAttributes) {
        Attributes = pAttributes;
    }

    public double CalculateDamage(DamageEffect pEffect, StatSystem pAttacker) {
        double damage = pEffect.Type switch {
            DamageEffect.DamageType.Physical => CalculatePhysicalDamage(pEffect, pAttacker),
            DamageEffect.DamageType.Shadow => 0.0d,
            DamageEffect.DamageType.Nature => 0.0d,
            DamageEffect.DamageType.Fire => 0.0d,
            _ => double.PositiveInfinity
        };

        if (double.IsPositiveInfinity(damage)) {
            Logger.Core.Critical($"Unhandled {nameof(DamageEffect.DamageType)} case.");
        }
        
        return damage;
    }

    private double CalculatePhysicalDamage(DamageEffect pEffect, StatSystem pAttacker) {
        return 0.0d;
    }
}