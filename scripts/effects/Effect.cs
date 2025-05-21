using System;
using RPG.global;
using Godot;

namespace RPG.scripts.effects;

[Tool, GlobalClass]
public sealed partial class Effect : ComponentSystem<EffectComponent> {
    [Signal]
    public delegate void OnTickEventHandler();

    [Flags]
    public enum EffectFlags {
        Immediate = 1 << 0,
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

    [Export] public long Range;
    [Export] public EffectFlags Flags;
    

    private int _ticks;
    private int _currentTick;

    public Timer? Start() {
        if (!RNG.IsSuccessfulRoll(ApplicationChance)) {
            return null;
        }

        if (_ticks == 0) {
            EmitSignalOnTick();
            return null;
        }

        if (IsImmediate()) {
            EmitSignalOnTick();
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

    private void SetupPeriodicEffect(Timer pTimer) {
        _currentTick--;

        if (_currentTick > 0) {
            EmitSignalOnTick();
            return;
        }

        if (!IsImmediate()) {
            EmitSignalOnTick();
        }

        pTimer.Stop();
        pTimer.QueueFree();
    }
}