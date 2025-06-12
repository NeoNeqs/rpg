using System;
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
        double damage = pDamageEffect.DamageType switch {
            DamageType.Physical => CalculatePhysicalDamage(pDamageEffect, pAttacker),
            DamageType.Shadow => CalculateShadowDamage(pDamageEffect, pAttacker),
            DamageType.Nature => 0.0d,
            DamageType.Fire => 0.0d,
            _ => double.PositiveInfinity
        };

        if (double.IsPositiveInfinity(damage)) {
            Logger.Core.Critical($"Unhandled {nameof(DamageType)} case.");
        }

        return damage;
    }

    // private double CalculateDamageReduction(
    //     Func<long, float> pDamageReductionFunc, 
    //     Func<long, float> pDamagePenetrationFunc,
    //     Stats.IntegerStat pReductionStat, 
    //     Stats.IntegerStat pPenetrationStat) {
    //     float damageReduction = pDamageReductionFunc(Stats.GetIntegerStat(pReductionStat));
    //     float shadowPenetration =
    //         pDamagePenetrationFunc(
    //             pAttacker.Stats.GetIntegerStat(Stats.IntegerStat.ShadowPenetration)
    //         );
    //     float totalDamageReduction = damageReduction + shadowPenetration;
    //
    //
    //     return pDamageEffect.GetTotalDamage(pAttacker.Stats) * totalDamageReduction;
    // }

    public long GetMaxHealth() {
        return StatCurves.GetHealthFromStamina(Stats.GetIntegerStat(Stats.IntegerStat.Stamina));
    }
    
    private double CalculateDamage(
        DamageEffectComponent pEffectComponent,
        CombatSystem pAttacker,
        Stats.IntegerStat pResistanceStat,
        Stats.IntegerStat pPenetrationStat,
        Func<long, float> pReductionFunc,
        Func<long, float> pPenetrationFunc
    ) {
        // pRawDamage = 100
        // damageReduction = 0.6
        // armorPenetration = 0.3
        // totalDamageReduction = damageReduction + armorPenetration = 0.6 + 0.3 = 0.9
        
        // damageReductionNoPen = pRawDamage * damageReduction = 100 * 0.6 = 60 damage taken
        // damageReductionWithPen = 100 * 0.9 = 90 damage taken
        
        // TL;DR: An attacker with armor penetration will deal more damage.
        
        float reduction = pReductionFunc(Stats.GetIntegerStat(pResistanceStat));
        float penetration = pPenetrationFunc(pAttacker.Stats.GetIntegerStat(pPenetrationStat));
        float totalReduction = reduction + penetration;

        return pEffectComponent.GetTotalDamage(pAttacker.Stats) * totalReduction;
    }

    private double CalculateShadowDamage(DamageEffectComponent pDamageEffect, CombatSystem pAttacker) {
        return CalculateDamage(
            pDamageEffect,
            pAttacker,
            Stats.IntegerStat.ShadowResistance,
            Stats.IntegerStat.ShadowPenetration,
            StatCurves.GetShadowDamageReduction,
            pAttacker.StatCurves.GetShadowDamagePenetration
        );
    }

    private double CalculatePhysicalDamage(DamageEffectComponent pEffectComponent, CombatSystem pAttacker) {
        return CalculateDamage(
            pEffectComponent,
            pAttacker,
            Stats.IntegerStat.Armor,
            Stats.IntegerStat.ArmorPenetration,
            StatCurves.GetPhysicalDamageReduction,
            pAttacker.StatCurves.GetPhysicalDamagePenetration
        );
    }
    
    private double CalculateNatureDamage(DamageEffectComponent pEffectComponent, CombatSystem pAttacker) {
        return CalculateDamage(
            pEffectComponent,
            pAttacker,
            Stats.IntegerStat.NatureResistance,
            Stats.IntegerStat.NaturePenetration,
            StatCurves.GetNatureDamageReduction,
            pAttacker.StatCurves.GetNatureDamagePenetration
        );
    }

    // private double CalculateShadowDamage(DamageEffectComponent pDamageEffect, CombatSystem pAttacker) {
    //     float damageReduction =
    //         StatCurves.GetShadowDamageReduction(Stats.GetIntegerStat(Stats.IntegerStat.ShadowResistance));
    //     float shadowPenetration =
    //         pAttacker.StatCurves.GetShadowDamagePenetration(
    //             pAttacker.Stats.GetIntegerStat(Stats.IntegerStat.ShadowPenetration)
    //         );
    //     float totalDamageReduction = damageReduction + shadowPenetration;
    //
    //
    //     return pDamageEffect.GetTotalDamage(pAttacker.Stats) * totalDamageReduction;
    // }
    //
    //
    // private double CalculatePhysicalDamage(DamageEffectComponent pEffectComponent, CombatSystem pAttacker) {
    //     float damageReduction = StatCurves.GetPhysicalDamageReduction(Stats.GetIntegerStat(Stats.IntegerStat.Armor));
    //     float armorPenetration =
    //         pAttacker.StatCurves.GetPhysicalDamagePenetration(
    //             pAttacker.Stats.GetIntegerStat(Stats.IntegerStat.ArmorPenetration)
    //         );
    //     float totalDamageReduction = damageReduction + armorPenetration;
    //

    //
    //     return pEffectComponent.GetTotalDamage(pAttacker.Stats) * totalDamageReduction;
    // }
}