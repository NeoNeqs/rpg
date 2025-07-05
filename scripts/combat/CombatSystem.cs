using System;
using Godot;
using RPG.global;
using RPG.global.enums;
using RPG.scripts.effects;

namespace RPG.scripts.combat;

[GlobalClass]
public partial class CombatSystem : Resource {
    [Export] public StatCurves StatCurves = new();

    private Stats _stats = null!;

    public void Initialize(Stats pStats) {
        _stats = pStats;
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
        return StatCurves.GetHealthFromStamina(_stats.GetIntegerStat(IntegerStat.Stamina));
    }

    private double CalculateDamage(
        DamageEffect pDamageEffect,
        CombatSystem pAttacker,
        IntegerStat pResistanceStat,
        IntegerStat pPenetrationStat,
        Func<long, float> pReductionFunc,
        Func<long, float> pPenetrationFunc
    ) {
        float reduction = pReductionFunc(_stats.GetIntegerStat(pResistanceStat));
        float penetration = pPenetrationFunc(pAttacker._stats.GetIntegerStat(pPenetrationStat));
        float finalReduction = penetration / reduction;
   
        float totalDamage = pDamageEffect.GetTotalDamage(pAttacker._stats) * finalReduction;
        Logger.Combat.Debug($"Total dmg: {totalDamage}, Flat dmg: {pDamageEffect.FlatValue} {pDamageEffect.DamageType}, Coefficient: {pDamageEffect.StatScaleCoefficient}, StatScale: {pDamageEffect.StatScale}, Target reduction: {reduction}, Source penetration: {penetration}, Final reduction: {finalReduction}.");

        return totalDamage;
    }

    private double CalculateShadowDamage(DamageEffect pDamageEffect, CombatSystem pAttacker) {
        return CalculateDamage(
            pDamageEffect,
            pAttacker,
            IntegerStat.ShadowResistance,
            IntegerStat.ShadowPenetration,
            StatCurves.GetShadowDamageReduction,
            pAttacker.StatCurves.GetShadowDamagePenetration
        );
    }

    private double CalculatePhysicalDamage(DamageEffect pDamageEffect, CombatSystem pAttacker) {
        return CalculateDamage(
            pDamageEffect,
            pAttacker,
            IntegerStat.Armor,
            IntegerStat.ArmorPenetration,
            StatCurves.GetPhysicalDamageReduction,
            pAttacker.StatCurves.GetPhysicalDamagePenetration
        );
    }

    private double CalculateNatureDamage(DamageEffect pDamageEffect, CombatSystem pAttacker) {
        return CalculateDamage(
            pDamageEffect,
            pAttacker,
            IntegerStat.NatureResistance,
            IntegerStat.NaturePenetration,
            StatCurves.GetNatureDamageReduction,
            pAttacker.StatCurves.GetNatureDamagePenetration
        );
    }
}