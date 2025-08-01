using RPG.global.enums;

namespace RPG.scripts.combat.strategies;

public class PhysicalDamageStrategy : IDamageStrategy {
    public IntegerStat ResistanceStat => IntegerStat.Armor;
    public IntegerStat PenetrationStat => IntegerStat.ArmorPenetration;

    public float GetReduction(StatCurves pCurves, long pValue) => pCurves.GetPhysicalDamageReduction(pValue);
    public float GetPenetration(StatCurves pCurves, long pValue) => pCurves.GetPhysicalDamagePenetration(pValue);
}