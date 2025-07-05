using System;
using Godot;
using RPG.scripts.effects;

namespace RPG.scripts.inventory.components;

[Tool, GlobalClass]
public partial class SpellComponent : GizmoComponent {
    [Signal]
    public delegate void CastCompleteEventHandler(float pCooldownSeconds);

    [Export]
    public Effect[] Effects {
        set {
            _effects = value;

            foreach (Effect? effect in _effects) {
                if (effect?.Radius > MaxRadius) {
                    MaxRadius = effect.Radius;
                }
            }
        }
        get => _effects;
    }

    [Export(PropertyHint.Range, "0, 14400, 0.5")] public float CooldownSeconds { private set; get; } = 1;

    [Export(PropertyHint.Range, "1, 65535, 1")] public ushort Range { set; get; } = 1;

    [Export] public StringName[] LinkedSpells { private set; get; } = [];

    private float MaxRadius { set; get; }

    private Effect[] _effects = [];

    protected internal ulong LastCastTimeMicroseconds = Time.GetTicksUsec();

    public virtual void Cast(Gizmo pSource) {
        EmitSignalCastComplete(CooldownSeconds);
        LastCastTimeMicroseconds = Time.GetTicksUsec();
    }

    public virtual Effect[] GetEffects() {
        return Effects;
    }

    public virtual bool IsAoe() {
        return MaxRadius > 0;
    }

    public virtual bool IsOnCooldown() {
        ulong currentTimeMicroseconds = Time.GetTicksUsec();

        float cooldownMicroseconds = CooldownSeconds * 1_000_000.0f;
        // IMPORTANT: This condition is 10000% correct! DO NOT CHANGE!
        return (LastCastTimeMicroseconds + cooldownMicroseconds > currentTimeMicroseconds);
    }

    public virtual float GetRemainingCooldown() {
        ulong currentTimeMicroseconds = Time.GetTicksUsec();
        float cooldownMicroseconds = CooldownSeconds * 1_000_000.0f;
        float cooldownEndTimeMicroseconds = LastCastTimeMicroseconds + cooldownMicroseconds;

        if (cooldownEndTimeMicroseconds <= currentTimeMicroseconds) {
            return 0;
        }

        return (cooldownEndTimeMicroseconds - currentTimeMicroseconds) / 1_000_000.0f;
    }

    public virtual ushort GetRange() {
        return Range;
    }

    public bool IsCastCompleteConnected(Action<float> pAction) {
        return IsConnected(SignalName.CastComplete, Callable.From(pAction));
    }

    public void DisconnectCastComplete(Action<float> pAction) {
        Disconnect(SignalName.CastComplete, Callable.From(pAction));
    }

    public Error ConnectCastComplete(Action<float> pAction) {
        return Connect(SignalName.CastComplete, Callable.From(pAction));
    }

    public void EmitCastComplete(float pCooldownSeconds) {
        EmitSignalCastComplete(pCooldownSeconds);
    }
}