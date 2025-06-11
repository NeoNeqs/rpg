using Godot;
using RPG.global;
using RPG.scripts.effects.components;

namespace RPG.scripts.combat;

[GlobalClass]
public partial class CombatSystem : Resource {

    [Export] public StatCurves StatCurves = null!;
    public required Stats Stats;
    
    public void Initialize(Stats pStats) {
        Stats = pStats;
    }

    public double CalculateDamage(DamageEffectComponent pDamageEffect, CombatSystem pAttacker) {
        double damage = pDamageEffect.Type switch {
            DamageType.Physical => CalculatePhysicalDamage(pDamageEffect.Value, pAttacker),
            DamageType.Shadow => 0.0d,
            DamageType.Nature => 0.0d,
            DamageType.Fire => 0.0d,
            _ => double.PositiveInfinity
        };

        if (double.IsPositiveInfinity(damage)) {
            Logger.Core.Critical($"Unhandled {nameof(DamageType)} case.");
        }
        
        return damage;
    }


    public long GetMaxHealth() {
        return StatCurves.GetHealthFromStamina(Stats.GetIntegerStat(Stats.IntegerStat.Stamina));
    }

    private double CalculatePhysicalDamage(long pRawDamage, CombatSystem pAttacker) {
        float damageReduction = StatCurves.GetPhysicalDamageReduction(Stats.GetIntegerStat(Stats.IntegerStat.Armor));
        float armorPenetration = pAttacker.StatCurves.GetPhysicalDamagePenetration(pAttacker.Stats.GetIntegerStat(Stats.IntegerStat.ArmorPenetration));
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