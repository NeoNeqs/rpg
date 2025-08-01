using Godot;
using RPG.global.tools;

namespace RPG.scripts.combat;

[Tool, GlobalClass]
public partial class StatCurves : Resource {
    private static readonly Curve DefaultReductionCurve = GD.Load<Curve>("uid://b64ayt2qovyp5");
    private static readonly Curve DefaultPenetrationCurve = GD.Load<Curve>("uid://o2uba1xx1m8y");
    private static readonly Curve DefaultStatCurve = GD.Load<Curve>("uid://btjbpfhu8xv7e");
    [Export] public Curve ArmorPenetrationCurve = (Curve)DefaultPenetrationCurve.Duplicate();

    // TODO: bake curves before use.

    [Export] public Curve ArmorReductionCurve = (Curve)DefaultReductionCurve.Duplicate();
    [Export] public Curve NaturePenetrationCurve = (Curve)DefaultPenetrationCurve.Duplicate();

    [Export] public Curve NatureReductionCurve = (Curve)DefaultReductionCurve.Duplicate();
    [Export] public Curve ShadowPenetrationCurve = (Curve)DefaultPenetrationCurve.Duplicate();

    [Export] public Curve ShadowReductionCurve = (Curve)DefaultReductionCurve.Duplicate();
    [Export] public Curve StaminaCurve = (Curve)DefaultStatCurve.Duplicate();

    [Export] public Curve StrengthCurve = (Curve)DefaultStatCurve.Duplicate();

#if TOOLS
    static StatCurves() {
        Tools.Assert(DefaultReductionCurve is not null, $"Missing {nameof(DefaultReductionCurve)}.");
        Tools.Assert(DefaultPenetrationCurve is not null, $"Missing {nameof(DefaultPenetrationCurve)}.");
        Tools.Assert(DefaultStatCurve is not null, $"Missing {nameof(DefaultStatCurve)}.");
        // Make sure uid system did not break resources.
        Tools.Assert(DefaultReductionCurve!.ResourcePath.GetFile().Contains("reduction"),
            $"Possible resource mismatch. Expected a {nameof(Curve)} resource that defines default damage reduction.");
        Tools.Assert(DefaultPenetrationCurve!.ResourcePath.GetFile().Contains("penetration"),
            $"Possible resource mismatch. Expected a {nameof(Curve)} resource that defines default damage penetration.");
        Tools.Assert(DefaultStatCurve!.ResourcePath.GetFile().Contains("stat"),
            $"Possible resource mismatch. Expected a {nameof(Curve)} resource that defines default stat values.");
    }
#endif

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