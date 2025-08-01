using System.Collections.Generic;
using Godot;
using RPG.global;
using RPG.global.enums;
using RPG.scripts.combat.strategies;
using RPG.scripts.effects;

namespace RPG.scripts.combat;

[GlobalClass]
public partial class StatCalculator : Resource {
    [Export] public StatCurves StatCurves = new();

    private Stats _stats = null!;
    
    private readonly Dictionary<DamageType, IDamageStrategy> _damageStrategies = new();

    public void Initialize(Stats pStats) {
        _stats = pStats;
        
        _damageStrategies[DamageType.Physical] = new PhysicalDamageStrategy();
        // _damageStrategies[DamageType.Shadow] = new ShadowDamageStrategy();
        // _damageStrategies[DamageType.Nature] = new NatureDamageStrategy();
    }
    
    public double CalculateDamage(DamageEffect pDamageEffect, StatCalculator pAttacker) {
        if (!_damageStrategies.TryGetValue(pDamageEffect.DamageType, out IDamageStrategy? strategy)) {
            Logger.Core.Critical($"Unhandled {nameof(DamageType)} case.", true);
            return 0.0d;
        }

        long resistance = _stats.GetIntegerStat(strategy.ResistanceStat);
        long penetration = pAttacker._stats.GetIntegerStat(strategy.PenetrationStat);

        float reduction = strategy.GetReduction(StatCurves, resistance);
        float penValue = strategy.GetPenetration(StatCurves, penetration);
        float finalReduction = penValue / reduction;

        float totalDamage = pDamageEffect.GetTotalDamage(pAttacker._stats) * finalReduction;

        Logger.Combat.Debug($"Total dmg: {totalDamage}, Flat dmg: {pDamageEffect.FlatValue} {pDamageEffect.DamageType}, Coefficient: {pDamageEffect.StatScaleCoefficient}, StatScale: {pDamageEffect.StatScale}, Target reduction: {reduction}, Source penetration: {penValue}, Final reduction: {finalReduction}.");

        return totalDamage;
    }
    

    public long GetMaxHealth() {
        return StatCurves.GetHealthFromStamina(_stats.GetIntegerStat(IntegerStat.Stamina));
    }
}