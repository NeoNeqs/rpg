using RPG.global.enums;

namespace RPG.scripts.combat.strategies;

public class PhysicalDamageStrategy : IDamageStrategy {
    public IntegerStat ResistanceStat => IntegerStat.Armor;
    public IntegerStat PenetrationStat => IntegerStat.ArmorPenetration;

    public float GetReduction(StatCurves pCurves, StatLinker pStatLinker) {
        return pCurves.GetPhysicalDamageReduction(pStatLinker.Total.GetIntegerStat(ResistanceStat));
    }

    public float GetPenetration(StatCurves pCurves, StatLinker pStatLinker) {
        return pCurves.GetPhysicalDamagePenetration(pStatLinker.Total.GetIntegerStat(PenetrationStat));
    }
}