using Godot;
using RPG.scripts.combat;

namespace RPG.scripts.effects.components;

public partial class AttributeEffectComponent : EffectComponent {
    [Export] public Attributes Attr = new();
}