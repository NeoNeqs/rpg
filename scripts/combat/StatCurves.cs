using Godot;

namespace RPG.scripts.combat;

[GlobalClass]
public partial class StatCurves : Resource {
    // TODO: bake curves before use.
    
    [Export] public Curve ArmorReductionCurve = null!;
    [Export] public Curve ArmorPenetrationCurve = null!;
    
    [Export] public Curve ShadowReductionCurve = null!;
    [Export] public Curve ShadowPenetrationCurve = null!;
    
    [Export] public Curve NatureReductionCurve = null!;
    [Export] public Curve NaturePenetrationCurve = null!;
    
    [Export] public Curve StrengthCurve = null!;
    [Export] public Curve StaminaCurve = null!;
    

    public long GetHealthFromStamina(long pStamina) {
        return (long)StaminaCurve.Sample(pStamina);
    }

    public float GetPhysicalDamageReduction(long pArmor) {
        return ArmorReductionCurve.Sample(pArmor);
    }

    public float GetPhysicalDamagePenetration(long pArmorPenetration) {
        return ArmorPenetrationCurve.Sample(pArmorPenetration);
    }

    public float GetShadowDamageReduction(long pShadowRes) {
        return ShadowReductionCurve.Sample(pShadowRes);
    }
    
    public float GetShadowDamagePenetration(long pShadowRes) {
        return ShadowPenetrationCurve.Sample(pShadowRes);
    }

    public float GetNatureDamageReduction(long pNatureRes) {
        return NatureReductionCurve.Sample(pNatureRes);
    }
    
    public float GetNatureDamagePenetration(long pNatureRes) {
        return NaturePenetrationCurve.Sample(pNatureRes);
    }
    

}