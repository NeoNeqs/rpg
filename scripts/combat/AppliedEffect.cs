using Godot;
using RPG.scripts.effects;
using RPG.scripts.inventory;

namespace RPG.scripts.combat;

public partial class AppliedEffect(Gizmo pEffectSource, Effect pEffect) : RefCounted {
    public Gizmo EffectSource => pEffectSource;
    public Effect Effect => pEffect;
}