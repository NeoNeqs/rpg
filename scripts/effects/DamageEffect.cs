using Godot;
using System;

namespace RPG.scripts.effects;

public partial class DamageEffect : Effect {
    [Signal]
    public delegate void TickEventHandler(DamageEffect pEffect);


    public enum DamageType {
        Physical,
        Shadow,
        Nature,
        Fire,
    }
    
    public DamageType Type;

    protected override void _Setup() {
        EmitSignalTick(this);
    }
}
