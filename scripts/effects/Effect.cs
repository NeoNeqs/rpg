using System;
using RPG.global;
using Godot;
using RPG.scripts.effects.components;

namespace RPG.scripts.effects;

[Tool, GlobalClass]
public sealed partial class Effect : ComponentSystem<EffectComponent> {
    [Signal]
    public delegate void TickEventHandler();
    [Signal]
    public delegate void FinishedEventHandler();

    [Flags]
    public enum EffectFlags {
        Immediate = 1 << 0,
        TargetSelf = 1 << 1,
        Buff = 1 << 2,
    }

    [Export] public int TickTimeout = 3;

    [Export(PropertyHint.Range, "0,1,0.01")]
    public double ApplicationChance = 1.0;

    [Export]
    public int Ticks {
        set {
            _ticks = value;
            _currentTick = value;
        }
        get => _ticks;
    }

    [Export] public long MaxRange;
    [Export] public EffectFlags Flags;

    private int _ticks;
    private int _currentTick;

    public Timer? Start() {
        if (!RNG.Roll(ApplicationChance)) {
            return null;
        }

        if (_ticks == 0) {
            EmitSignalTick();
            return null;
        }

        if (IsImmediate()) {
            EmitSignalTick();
        }

        var timer = new Timer();
        timer.Autostart = true;
        timer.Timeout += () => SetupPeriodicEffect(timer);
        timer.WaitTime = TickTimeout;

        return timer;
    }

    private bool IsImmediate() {
        return (Flags & EffectFlags.Immediate) == 0;
    }

    public bool IsTargetSelf() {
        return (Flags & EffectFlags.TargetSelf) == 0;
    }

    public bool IsBuff() {
        return (Flags & EffectFlags.Buff) == 0;
    }

    private void SetupPeriodicEffect(Timer pTimer) {
        _currentTick--;

        if (_currentTick > 0) {
            EmitSignalTick();
            return;
        }

        if (!IsImmediate()) {
            EmitSignalTick();
        }

        pTimer.Stop();
        pTimer.QueueFree();
        EmitSignalFinished();
    }
}