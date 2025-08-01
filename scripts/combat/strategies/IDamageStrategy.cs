using RPG.global.enums;

namespace RPG.scripts.combat.strategies;

public interface IDamageStrategy {
    IntegerStat ResistanceStat { get; }
    IntegerStat PenetrationStat { get; }

    float GetReduction(StatCurves pCurves, StatLinker pStatLinker);
    float GetPenetration(StatCurves pCurves, StatLinker pStatLinker);
}