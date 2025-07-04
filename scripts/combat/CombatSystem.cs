using System;
using Godot;
using RPG.global;
using RPG.global.enums;
using RPG.scripts.effects;

namespace RPG.scripts.combat;

[GlobalClass]
public partial class CombatSystem : Resource {
    [Export] public StatCurves StatCurves = new();

    // ReSharper disable once InconsistentNaming
    private Stats Stats = null!;

    public void Initialize(Stats pStats) {
        Stats = pStats;
    }

    public double CalculateDamage(DamageEffect pDamageEffect, CombatSystem pAttacker) {
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
        DamageEffect pDamageEffect,
        CombatSystem pAttacker,
        Stats.IntegerStat pResistanceStat,
        Stats.IntegerStat pPenetrationStat,
        Func<long, float> pReductionFunc,
        Func<long, float> pPenetrationFunc
    ) {
        float reduction = pReductionFunc(Stats.GetIntegerStat(pResistanceStat));
        float penetration = pPenetrationFunc(pAttacker.Stats.GetIntegerStat(pPenetrationStat));
        float finalReduction = penetration / reduction;
   
        float totalDamage = pDamageEffect.GetTotalDamage(pAttacker.Stats) * finalReduction;
        Logger.Combat.Debug($"Total dmg: {totalDamage}, Flat dmg: {pDamageEffect.FlatValue} {pDamageEffect.DamageType}, Coefficient: {pDamageEffect.StatScaleCoefficient}, StatScale: {pDamageEffect.StatScale}, Target reduction: {reduction}, Source penetration: {penetration}, Final reduction: {finalReduction}.");

        return totalDamage;
    }

    private double CalculateShadowDamage(DamageEffect pDamageEffect, CombatSystem pAttacker) {
        return CalculateDamage(
            pDamageEffect,
            pAttacker,
            Stats.IntegerStat.ShadowResistance,
            Stats.IntegerStat.ShadowPenetration,
            StatCurves.GetShadowDamageReduction,
            pAttacker.StatCurves.GetShadowDamagePenetration
        );
    }

    private double CalculatePhysicalDamage(DamageEffect pDamageEffect, CombatSystem pAttacker) {
        return CalculateDamage(
            pDamageEffect,
            pAttacker,
            Stats.IntegerStat.Armor,
            Stats.IntegerStat.ArmorPenetration,
            StatCurves.GetPhysicalDamageReduction,
            pAttacker.StatCurves.GetPhysicalDamagePenetration
        );
    }

    private double CalculateNatureDamage(DamageEffect pDamageEffect, CombatSystem pAttacker) {
        return CalculateDamage(
            pDamageEffect,
            pAttacker,
            Stats.IntegerStat.NatureResistance,
            Stats.IntegerStat.NaturePenetration,
            StatCurves.GetNatureDamageReduction,
            pAttacker.StatCurves.GetNatureDamagePenetration
        );
    }
}