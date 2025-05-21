using System;
using Godot;
using RPG.Global;
using RPG.scripts.effects;
using RPG.scripts.effects.components;

namespace RPG.scripts.combat;

public partial class StatSystem : Resource {

    [Export] public Attributes Attributes = null!;
    [Export] public Stats Stats = null!;
    
    public void Initialize(Attributes pAttributes) {
        Attributes = pAttributes;
    }

    public double CalculateDamage(DamageEffectComponent pDamageEffect, StatSystem pAttacker) {
        double damage = pDamageEffect.Type switch {
            DamageEffectComponent.DamageType.Physical => CalculatePhysicalDamage(pDamageEffect.Value, pAttacker),
            DamageEffectComponent.DamageType.Shadow => 0.0d,
            DamageEffectComponent.DamageType.Nature => 0.0d,
            DamageEffectComponent.DamageType.Fire => 0.0d,
            _ => double.PositiveInfinity
        };

        if (double.IsPositiveInfinity(damage)) {
            Logger.Core.Critical($"Unhandled {nameof(DamageEffectComponent.DamageType)} case.");
        }
        
        return damage;
    }


    public long GetMaxHealth() {
        return Stats.GetHealthFromStamina(Attributes.Stamina);
    }

    private double CalculatePhysicalDamage(long pRawDamage, StatSystem pAttacker) {
        float damageReduction = Stats.GetPhysicalDamageReduction(Attributes.Armor);
        float armorPenetration = pAttacker.Stats.GetPhysicalDamagePenetration(pAttacker.Attributes.ArmorPenetration);
        float totalDamageReduction = damageReduction + armorPenetration;
        
        // pRawDamage = 100
        // damageReduction = 0.6
        // armorPenetration = 0.3
        // totalDamageReduction = damageReduction + armorPenetration = 0.6 + 0.3 = 0.9
        
        // damageReductionNoPen = pRawDamage * damageReduction = 100 * 0.6 = 60 damage taken
        // damageReductionWithPen = 100 * 0.9 = 90 damage taken
        
        // TL;DR: An attacker with armor penetration will deal more damage.
        
        return pRawDamage * totalDamageReduction;
    }
}