using Godot;
using RPG.scripts.combat;

namespace RPG.scripts.effects;

public partial class AttributeEffect : Effect {
    
    [Signal]
    public delegate void TickEventHandler(AttributeEffect pEffect);

    [Export] public required Attributes Attributes;
    
    
    protected override void SetupImpl() {
        EmitSignalTick(this);
    }
}