using System;
using Godot;
using RPG.scripts.effects;
using RPG.scripts.effects.components;

namespace RPG.scripts.inventory.components;

[Tool, GlobalClass]
public partial class SpellComponent : GizmoComponent {
    [Signal]
    public delegate void CastCompleteEventHandler(ulong pCooldownInMicroSeconds);

    [Export]
    public Effect[] Effects {
        private set {
            _effects = value;

            foreach (Effect effect in _effects) {
                var aoeEffectComponent = effect?.GetComponent<AreaOfEffectComponent>();
                if (aoeEffectComponent is not null) {
                    IsAoe = true;
                    if (aoeEffectComponent.Radius > MaxRadius) {
                        MaxRadius = aoeEffectComponent.Radius;
                    }
                }

                if (effect?.Range > MaxRange) {
                    MaxRange = effect.Range;
                }
            }
        }
        get => _effects;
    }

    [Export] public ulong CooldownSeconds { private set; get; } = 1;

    public bool IsAoe { private set; get; }
    public float MaxRadius { private set; get; }
    public float MaxRange { private set; get; }


    private Effect[] _effects = null!;
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
        // This condition is 10000% correct! DO NOT CHANGE!
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


    // I hate working with Godot signals in C#
    // Why must I redefine those methods to use them outside the class :/
    // Waiting for Godot...
    // https://github.com/godotengine/godot-proposals/issues/12269
    // My guess is 2030
    public void EmitCastComplete(ulong pCooldownInMicroSeconds) {
        EmitSignalCastComplete(pCooldownInMicroSeconds);
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