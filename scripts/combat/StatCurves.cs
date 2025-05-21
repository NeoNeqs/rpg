using Godot;

namespace RPG.scripts.combat;

[GlobalClass]
public partial class StatCurves : Resource {
    // TODO: bake curves before use.
    
    [Export] public Curve ArmorEffectivenessCurve = null!;
    [Export] public Curve ArmorPenetrationCurve = null!;
    
    [Export] public Curve StrengthCurve = null!;
    [Export] public Curve StaminaCurve = null!;


    public long GetHealthFromStamina(long pStamina) {
        return (long)StaminaCurve.Sample(pStamina);
    }

    public float GetPhysicalDamageReduction(long pArmor) {
        return ArmorEffectivenessCurve.Sample(pArmor);
    }

    public float GetPhysicalDamagePenetration(long pArmorPenetration) {
        return ArmorPenetrationCurve.Sample(pArmorPenetration);
    }
}