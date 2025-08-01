using System.Collections.Generic;
using Godot;
using RPG.global.enums;
using RPG.scripts.combat.strategies;
using RPG.scripts.effects;

namespace RPG.scripts.combat;

[GlobalClass]
public partial class StatCalculator : Resource {
    private static readonly Dictionary<DamageType, IDamageStrategy> DamageStrategies = new() {
        { DamageType.Physical, new PhysicalDamageStrategy() }
    };

    private StatLinker _statLinker = null!;
    [Export] public StatCurves StatCurves = new();

    public void Initialize(StatLinker pStatLinker) {
        _statLinker = pStatLinker;
    }

    public double CalculateDamage(DamageEffect pDamageEffect, StatCalculator pAttacker) {
        if (!DamageStrategies.TryGetValue(pDamageEffect.DamageType, out IDamageStrategy? strategy)) {
            Logger.Core.Critical($"Unhandled {pDamageEffect.DamageType.ToString()}.", true);
            return 0.0d;
        }

        float damageReduction = strategy.GetReduction(StatCurves, _statLinker);
        float attackerPenetration = strategy.GetPenetration(pAttacker.StatCurves, pAttacker._statLinker);
        float finalReduction = attackerPenetration / damageReduction;

        float totalDamage = pDamageEffect.GetTotalDamage(pAttacker._statLinker.Total) * finalReduction;

        Logger.Combat.Debug(
            $"Total dmg: {totalDamage}, Flat dmg: {pDamageEffect.FlatValue} {pDamageEffect.DamageType}, Coefficient: {pDamageEffect.StatScaleCoefficient}, StatScale: {pDamageEffect.StatScale}, Target reduction: {damageReduction}, Source penetration: {attackerPenetration}, Final reduction: {finalReduction}.");

        return totalDamage;
    }

    public long GetMaxHealth() {
        return StatCurves.GetHealthFromStamina(_statLinker.Total.GetIntegerStat(IntegerStat.Vigor));
    }
}