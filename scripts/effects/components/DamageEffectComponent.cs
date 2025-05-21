using Godot;

namespace RPG.scripts.effects.components;

public partial class DamageEffectComponent : EffectComponent {
    public enum DamageType {
        Physical,
        Shadow,
        Nature,
        Fire,
    }
    
    [Export]
    public DamageType Type;
    
    [Export]
    public long Value;
}