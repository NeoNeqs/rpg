using System;
using RPG.global;
using Godot;
using RPG.scripts.effects;

namespace RPG.scripts.inventory.components;

[Tool, GlobalClass]
public partial class SpellComponent : GizmoComponent {
    [Signal]
    public delegate void CastCompleteEventHandler(ulong pCooldownInMicroSeconds);

    [Export]
    public Effect[] Effects {
        private set {
            _effects = value;

            foreach (Effect? effect in _effects) {
                if (effect?.Radius > MaxRadius) {
                    MaxRadius = effect.Radius;
                }
            }
        }
        get => _effects;
    }

    [Export]
    public ulong CooldownSeconds { private set; get; } = 1;

    [Export(PropertyHint.Range, "1, 65535, 1")]
    public ushort Range { private set; get; } = 1;

    public bool IsAoe => MaxRadius > 0;
    private float MaxRadius { set; get; }

    private Effect[] _effects = [];
    protected ulong LastCastTime = Time.GetTicksUsec();

    public enum CastResult {
        Ok,
        OnCooldown,
        NoSpellFound,
    }

    public virtual CastResult Cast(Gizmo pSource) {
        if (IsOnCooldown()) {
            return CastResult.OnCooldown;
        }

        EmitSignalCastComplete(CooldownSeconds * 1_000_000);
        LastCastTime = Time.GetTicksUsec();

        return CastResult.Ok;
    }

    public bool IsOnCooldown() {
        ulong currentTime = Time.GetTicksUsec();

        ulong cooldownMicroseconds = CooldownSeconds * 1_000_000;
        // IMPORTANT: This condition is 10000% correct! DO NOT CHANGE!
        return (LastCastTime + cooldownMicroseconds > currentTime);
    }

    public ulong GetRemainingCooldown() {
        ulong currentTime = Time.GetTicksUsec();
        ulong cooldownMicroseconds = CooldownSeconds * 1_000_000;
        if (LastCastTime + cooldownMicroseconds <= currentTime) {
            return 0;
        }

        return (LastCastTime + cooldownMicroseconds - currentTime);
    }

    public bool IsCastCompleteConnected(Action<ulong> pAction) {
        return IsConnected(SignalName.CastComplete, Callable.From(pAction));
    }

    public void DisconnectCastComplete(Action<ulong> pAction) {
        Disconnect(SignalName.CastComplete, Callable.From(pAction));
    }

    public Error ConnectCastComplete(Action<ulong> pAction) {
        return Connect(SignalName.CastComplete, Callable.From(pAction));
    }
}