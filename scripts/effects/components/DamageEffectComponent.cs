using RPG.global;
using Godot;

namespace RPG.scripts.effects.components;

[Tool, GlobalClass] 
public partial class DamageEffectComponent : EffectComponent {
    [Export] public DamageType Type;

    [Export] public long Value;
}