using Godot;

namespace RPG.scripts.combat;

public partial class Stats : Resource {
    [Export] public Curve ArmorEffectivenessCurve { get; set; }
    [Export] public required Curve ArmorPenetrationCurve;
    
    [Export] public required Curve StrengthCurve;
    [Export] public required Curve StaminaCurve;
}