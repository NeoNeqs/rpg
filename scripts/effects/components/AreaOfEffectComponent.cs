using Godot;

namespace RPG.scripts.effects.components;

public partial class AreaOfEffectComponent : EffectComponent {
    [Export] public float Radius;
    [Export] public float MaxDistance;
}