using Godot;

namespace RPG.scripts.effects.components;

[Tool, GlobalClass]
public partial class AreaOfEffectComponent : EffectComponent {
    [Export] public float Radius;
    [Export] public float MaxDistance;
}