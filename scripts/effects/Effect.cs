using System;
using RPG.global;
using Godot;

namespace RPG.scripts.effects;

public abstract partial class Effect : Resource {
    [Flags]
    public enum EffectFlags {
        Immediate = 1 << 0,
    }

    [Export] public long Value;
    [Export] public int TickTimeout = 3;

    [Export(PropertyHint.Range, "0,1,0.01")]
    public double ApplicationChance;

    [Export]
    public int Ticks {
        set {
            _ticks = value;
            _currentTick = value;
        }
        get => _ticks;
    }

    [Export] public EffectFlags Flags;

    private int _ticks;
    private int _currentTick;

    public Timer? Setup() {
        if (!RNG.IsSuccessfulRoll(ApplicationChance)) {
            return null;
        }

        if (_ticks == 0) {
            SetupImpl();
            return null;
        }

        if (IsImmediate()) {
            SetupImpl();
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
            SetupImpl();
            return;
        }

        if (!IsImmediate()) {
            SetupImpl();
        }

        Kill(pTimer);
    }


    private void Kill(Timer pTimer) {
        pTimer.Stop();
        pTimer.QueueFree();
    }

    protected abstract void SetupImpl();
}