using System;
using Godot;
using Godot.Collections;
using RPG.global.singletons;
using RPG.scripts.effects;

namespace RPG.scripts.inventory.components;

[Tool, GlobalClass]
public partial class SpellComponent : GizmoComponent {
    [Signal]
    public delegate void CastCompleteEventHandler(float pCooldownSeconds);

    public Effect[] Effects { private set; get; } = [];

    public float CooldownSeconds { private set; get; } = 1;

    public ushort Range { private set; get; } = 1;

    public float CastTimeSeconds { private set; get; } = 0;

    public StringName[] LinkedSpells { private set; get; } = [];

    protected internal ulong LastCastTimeMicroseconds = Time.GetTicksUsec();

    public virtual void Cast(Gizmo pSource) {
        BaseCast();
    }

    public void BaseCast() {
        EmitSignalCastComplete(CooldownSeconds);
        LastCastTimeMicroseconds = Time.GetTicksUsec();
    }

    public virtual Effect[] GetEffects() {
        return Effects;
    }

    public virtual bool IsAoe() {
        foreach (Effect? effect in Effects) {
            if (effect?.IsAoe() ?? false) {
                return true;
            }
        }

        return false;
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

    public override Array<Dictionary> _GetPropertyList() {
        return [
            new Dictionary {
                ["name"] = nameof(Effects),
                ["type"] = Variant.From(Variant.Type.Array),
                ["hint"] = Variant.From(PropertyHint.TypeString),
                ["hint_string"] = $"{Variant.Type.Object:D}/{PropertyHint.ResourceType:D}:{nameof(Effect)}",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary {
                ["name"] = nameof(CooldownSeconds),
                ["type"] = Variant.From(Variant.Type.Float),
                ["hint"] = Variant.From(PropertyHint.Range),
                ["hint_string"] = "0, 14400, 0.5",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary {
                ["name"] = nameof(Range),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Range),
                ["hint_string"] = $"1, {ushort.MaxValue}, 1",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary {
                ["name"] = nameof(CastTimeSeconds),
                ["type"] = Variant.From(Variant.Type.Float),
                ["hint"] = Variant.From(PropertyHint.Range),
                ["hint_string"] = $"1, 10, 0.5",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary() {
                ["name"] = nameof(LinkedSpells),
                ["type"] = Variant.From(Variant.Type.Array),
                ["hint"] = Variant.From(PropertyHint.TypeString),
                ["hint_string"] =
                    $"{Variant.Type.StringName:D}/{PropertyHint.EnumSuggestion:D}:{ResourceDB.SpellIdsHintString}",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
        ];
    }
}