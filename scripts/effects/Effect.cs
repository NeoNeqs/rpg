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
        // Immediate = 1 << 0,
        // TargetSelf = 1 << 1,
        Buff = 1 << 2,
    }

    // Note: TickTimeout refers to the time in seconds it takes to 1 application of the effect
    [Export(PropertyHint.Range, "0, 100, 1")]
    public int TickTimeout { private set; get; } = 3;

    [Export(PropertyHint.Range, "0, 1, 0.01")]
    public double ApplicationChance { private set; get; } = 1.0;

    // Note: Ticks refers to the amount of applications of the effect
    [Export(PropertyHint.Range, "1, 100, 1")]
    public int Ticks {
        private set {
            _ticks = value;
            _currentTick = value;
        }
        get => _ticks;
    }

    [Export(PropertyHint.Range, "1, 1000, 1")]
    public long Range { private set; get; } = 1;

    [Export] public EffectFlags Flags { private set; get; }

    private int _ticks = 1;
    private int _currentTick = 1;

    public Timer? Start() {
        if (!RNG.Roll(ApplicationChance)) {
            return null;
        }

        var timer = new Timer();
        timer.Autostart = true;
        timer.WaitTime = TickTimeout;

        if (_ticks == 1 /*|| IsImmediate()*/) {
            timer.Ready += () => SetupPeriodicEffect(timer);
        } else {
            timer.Timeout += () => SetupPeriodicEffect(timer);
        }

        return timer;
    }

    // public bool IsImmediate() {
    //     return (Flags ^ EffectFlags.Immediate) == 0;
    // }

    // public bool IsTargetSelf() {
    //     return (Flags ^ EffectFlags.TargetSelf) == 0;
    // }

    public bool IsBuff() {
        return (Flags ^ EffectFlags.Buff) == 0;
    }

    private void SetupPeriodicEffect(Timer pTimer) {
        if (_currentTick > 0) {
            EmitSignalTick();
            _currentTick--;
        }

        if (_currentTick == 0) {
            pTimer.Stop();
            pTimer.QueueFree();
            EmitSignalFinished();
        }
    }
}