using Godot;
using RPG.scripts.combat;

namespace RPG.scripts.effects.components;

[Tool, GlobalClass]
public partial class StatEffectComponent : EffectComponent {
    // Note to self: As of now EffectComponents are never duplicated, only the Effect itself is duplicated right before it's applied to an Entity.
    //               As long as none of the EffectComponents don't define any behaviors that needs to be tracked in through a property here, the component will not require duplication ever.
    //               Exported properties are exception here, their setters are made private so that the Engine / Inspector can only change them.
    //               They "basically" remain "constant" throughout the lifetime of the object.
    [Export] public long FlatValue { private set; get; }
    [Export] public Stats.IntegerStat Stat { private set; get; }
    
    [Export] public Stats.IntegerStat StatScale { private set; get; }
    [Export] public float Coefficient { private set; get; }
    
    [Export] public short MaxStacks { private set; get; } = 1;
    [Export] public Texture2D Icon { private set; get; } = null!;
}