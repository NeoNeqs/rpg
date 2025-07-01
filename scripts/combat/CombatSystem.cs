using System;
using Godot;
using RPG.global;
using RPG.global.enums;
using RPG.scripts.effects.components;

namespace RPG.scripts.combat;

[GlobalClass]
public partial class CombatSystem : Resource {
    [Export] public StatCurves StatCurves = null!;

    // ReSharper disable once InconsistentNaming
    private Stats Stats = null!;

    public void Initialize(Stats pStats) {
        Stats = pStats;
    }

    public double CalculateDamage(DamageEffectComponent pDamageEffect, CombatSystem pAttacker) {
        double damage = pDamageEffect.DamageType switch {
            DamageType.Physical => CalculatePhysicalDamage(pDamageEffect, pAttacker),
            DamageType.Shadow => CalculateShadowDamage(pDamageEffect, pAttacker),
            DamageType.Nature => CalculateNatureDamage(pDamageEffect, pAttacker),
            DamageType.Fire => 0.0d,
            _ => double.PositiveInfinity
        };

        if (double.IsPositiveInfinity(damage)) {
            Logger.Core.Critical($"Unhandled {nameof(DamageType)} case.", true);
        }


        return damage;
    }

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
        // reduction = 0.6
        // penetration = 0.3
        // totalDamageReduction = reduction + penetration = 0.6 + 0.3 = 0.9

        // damageReductionNoPen = pRawDamage * damageReduction = 100 * 0.6 = 60 damage taken
        // damageReductionWithPen = 100 * 0.9 = 90 damage taken

        // TL;DR: An attacker with armor penetration will deal more damage.

        float reduction = pReductionFunc(Stats.GetIntegerStat(pResistanceStat));
        float penetration = pPenetrationFunc(pAttacker.Stats.GetIntegerStat(pPenetrationStat));
        float totalReduction = reduction + penetration;

        float totalDamage = pEffectComponent.GetTotalDamage(pAttacker.Stats) * totalReduction;
        Logger.Combat.Info(
            $"Calculated damage '{totalDamage}' from flat damage '{pEffectComponent.FlatValue}' of '{pEffectComponent.DamageType}' and coefficient '{pEffectComponent.Coefficient}' scaling from '{pEffectComponent.StatScale}'.");

        return totalDamage;
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
}