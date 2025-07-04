using System;
using Godot;
using Godot.Collections;
using RPG.global;

namespace RPG.scripts.effects;

// TODO: I need to be able to apply an effect for 1 tick, but remove it after TickTimeout seconds! so the IsImmediate needs to come back.
// TODO: Create test spells:
//       - 20 second buff that modifies a stat Immediately and clears it only after 20 seconds have passed
//       - Deadly Poison - stacking poison up to 5 times that deals 30 damage every 3 seconds
[Tool, GlobalClass]
public partial class Effect : Resource, INamedIdentifiable {
    [Signal]
    public delegate void TickEventHandler();

    [Signal]
    public delegate void FinishedEventHandler();

    [Flags]
    public enum EffectFlags : ulong {
        TargetAllies = 1 << 0,
        TargetSelfOnly = 1 << 1,

        Instant = 1 << 15,
    }

// #if TOOLS
//     if (value.IsEmpty) {
//         _id = DisplayName.ToSnakeCase();
//         return;
//     }
// #endif

    public StringName Id { private set; get; } = new("");
    public string DisplayName { private set; get; } = "";

    public Texture2D? Icon { private set; get; }

    // Note: TickTimeout refers to the time in seconds it takes to 1 application of the effect
    public ushort TickTimeout { private set; get; } = 3;

    // Note: Ticks refers to the amount of applications of the effect
    public short Ticks {
        private set {
            _ticks = value;
            _currentTick = value;
        }
        get => _ticks;
    }

    public double ApplicationChance { set; get; } = 1.0;

    // Radius=0 means it's not an Aoe effect, Radius > 0 means IT IS an Aoe effect
    public ushort Radius { private set; get; }

    public EffectFlags Flags { private set; get; }

    protected Timer? Timer { private set; get; }

    // private StringName _id = new("");
    private short _ticks = 1;
    private short _currentTick = 1;

    public Timer? Start() {
        if (!RNG.Roll(ApplicationChance)) {
            return null;
        }

        if (Timer is not null && !Timer.IsStopped()) {
            return Timer;
        }

        Timer = new Timer();
        Timer.Autostart = true;
        Timer.WaitTime = TickTimeout;

        if (IsInstant()) {
            Timer.Ready += SetupPeriodicEffect;
        }

        Timer.Timeout += SetupPeriodicEffect;
        return Timer;
    }

    private bool IsInstant() {
        return (Flags ^ EffectFlags.Instant) == 0;
    }

    public bool IsTargetSelfOnly() {
        return (Flags ^ EffectFlags.TargetSelfOnly) == 0;
    }

    public bool IsTargetAllies() {
        return (Flags ^ EffectFlags.TargetAllies) == 0;
    }

    private void SetupPeriodicEffect() {
        _currentTick--;

        if (_currentTick < 0) {
            CleanupAndFinish();
            return;
        }

        EmitSignalTick();

        if (_currentTick == 0 && !IsInstant()) {
            CleanupAndFinish();
        }
    }

    private void CleanupAndFinish() {
#if TOOLS
        if (Timer is null) {
            Logger.Core.Critical("Timer should not be null here, but it is!", true);
            return;
        }
#endif
        Timer?.Stop();
        Timer?.QueueFree();
        EmitSignalFinished();
    }

    public override Array<Dictionary> _GetPropertyList() {
        return [
            new Dictionary() {
                ["name"] = nameof(Id),
                ["type"] = Variant.From(Variant.Type.StringName),
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary() {
                ["name"] = nameof(DisplayName),
                ["type"] = Variant.From(Variant.Type.String),
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary() {
                ["name"] = nameof(Icon),
                ["type"] = Variant.From(Variant.Type.Object),
                ["hint"] = Variant.From(PropertyHint.ResourceType),
                ["hint_string"] = "Texture2D",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary() {
                ["name"] = nameof(TickTimeout),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Range),
                ["hint_string"] = $"0, {ushort.MaxValue.ToString()}, 1",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary() {
                ["name"] = nameof(Ticks),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Range),
                ["hint_string"] = $"1, {short.MaxValue.ToString()}, 1",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary() {
                ["name"] = nameof(ApplicationChance),
                ["type"] = Variant.From(Variant.Type.Float),
                ["hint"] = Variant.From(PropertyHint.Range),
                ["hint_string"] = "0, 1, 0.01",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary() {
                ["name"] = nameof(Radius),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Range),
                ["hint_string"] = $"0, {ushort.MaxValue.ToString()}, 1",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary() {
                ["name"] = nameof(Flags),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Flags),
                ["hint_string"] = Utils.EnumToHintString<EffectFlags>(),
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
        ];
    }
}