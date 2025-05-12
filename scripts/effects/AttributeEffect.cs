using Godot;
using System;

namespace RPG.scripts.effects;

public partial class AttributeEffect : Effect {
    
    [Signal]
    public delegate void TickEventHandler(AttributeEffect pEffect);
    
    protected override void _Setup() {
        EmitSignalTick(this);
    }
}