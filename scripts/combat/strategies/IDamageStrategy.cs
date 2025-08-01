using RPG.global.enums;

namespace RPG.scripts.combat.strategies;

public interface IDamageStrategy {
    IntegerStat ResistanceStat { get; }
    IntegerStat PenetrationStat { get; }

    float GetReduction(StatCurves pCurves, long pValue);
    float GetPenetration(StatCurves pCurves, long pValue);
}