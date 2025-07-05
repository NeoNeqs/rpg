using Godot;
using Godot.Collections;
using RPG.global;
using RPG.global.enums;

namespace RPG.scripts.effects;

[Tool, GlobalClass]
public partial class Effect : Resource, INamedIdentifiable {
    [Signal]
    public delegate void TickEventHandler();

    [Signal]
    public delegate void FinishedEventHandler();

    public StringName Id { private set; get; } = new("");
    public string DisplayName { private set; get; } = "";

    public Texture2D? Icon { private set; get; }

    // Note: TickTimeout refers to the time in seconds it takes to 1 application of the effect
    public ushort TickTimeout { private set; get; } = 3;

    // Note: Ticks refers to the amount of applications of the effect
    public short TotalTicks {
        private set {
            _totalTicks = value;
            CurrentTick = value;
        }
        get => _totalTicks;
    }

    public float ApplicationChance { set; get; } = 1.0f;

    public ushort Radius { private set; get; }

    public EffectFlags Flags { private set; get; }

    public StringName[] ExcludingEffects { private set; get; } = [];

    public bool IsAoe => Radius > 0;
    protected Timer? Timer { private set; get; }

    private short _totalTicks = 1;
    protected short CurrentTick = 1;

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

    public bool IsInstant() {
        return Flags.HasFlag(EffectFlags.Instant);
    }

    public bool IsTargetSelfOnly() {
        return Flags.HasFlag(EffectFlags.TargetSelfOnly);
    }

    public bool IsTargetAllies() {
        return Flags.HasFlag(EffectFlags.TargetAllies);
    }

    private void SetupPeriodicEffect() {
        CurrentTick--;

        if (CurrentTick < 0) {
            CleanupAndFinish();
            return;
        }

        EmitSignalTick();

        if ((CurrentTick == 0 && !IsInstant()) || (TotalTicks == 1 && IsInstant())) {
            CleanupAndFinish();
        }
    }

    public void CleanupAndFinish() {
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

    public virtual void Refresh() {
        // Reset the timer since new stack was just applied
        if (Timer?.IsInsideTree() ?? false) {
            Timer.Start();
        }

        CurrentTick = TotalTicks;
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
                ["name"] = nameof(TotalTicks),
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
            new Dictionary() {
                ["name"] = nameof(ExcludingEffects),
                ["type"] = Variant.From(Variant.Type.Array),
                ["hint"] = Variant.From(PropertyHint.TypeString),
                ["hint_string"] = $"{Variant.Type.StringName:D}:",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
        ];
    }
}