using global::RPG.global;
using Godot;
using Godot.Collections;
using RPG.global.enums;
using RPG.global.singletons;

namespace RPG.scripts.effects;

[Tool, GlobalClass]
public partial class Effect : Resource, INamedIdentifiable {
    [Signal]
    public delegate void FinishedEventHandler();

    [Signal]
    public delegate void TickEventHandler();

    private short _totalTicks = 1;
    protected short CurrentTick = 1;

    // TODO: move this to INamedIdentifiable and rename that interface to something that could hold id, name and icon
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
    public EffectFlags Flags { private set; get; }
    public StringName[] ExcludingEffects { private set; get; } = [];
    public EffectBehavior? Behavior { private set; get; }
    public Timer Timer { private set; get; } = new();

    public StringName Id { private set; get; } = new("");
    public string DisplayName { private set; get; } = "";

    public Timer TryStart() {
        if (!Timer.IsStopped()) {
            return Timer;
        }

        Timer.Autostart = true;
        Timer.WaitTime = TickTimeout;
        CurrentTick = TotalTicks;

        if (IsInstant()) {
            Timer.Ready += TickFunction;
        }

        Timer.Timeout += TickFunction;
        return Timer;
    }

    public float GetTimeLeft() {
        float timeLeft = (CurrentTick - 1) * TickTimeout;

        if (IsInstanceValid(Timer) && !Timer.IsStopped()) {
            timeLeft += (float)Timer.TimeLeft;
        } else {
            timeLeft += TickTimeout;
        }

        if (timeLeft < 0.0f) {
            return 0.0f;
        }

        return timeLeft;
    }

    public bool IsInstant() {
        return Flags.HasFlag(EffectFlags.Instant);
    }

    public bool TargetsEnemies() {
        return Flags.HasFlag(EffectFlags.TargetsEnemies);
    }

    public bool IsAoe() {
        return Behavior is AoeEffectBehavior;
    }

    private void TickFunction() {
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
        Timer.Stop();
        Timer.GetParent().RemoveChild(Timer);
        Timer.Owner = null;
        EmitSignalFinished();
    }

    public virtual void Refresh() {
        // Reset the timer since new stack was just applied
        if (Timer.IsInsideTree()) {
            Timer.Start();
        }

        CurrentTick = TotalTicks;
    }

    public override Array<Dictionary> _GetPropertyList() {
        return [
            new Dictionary {
                ["name"] = nameof(Id),
                ["type"] = Variant.From(Variant.Type.StringName),
                ["usage"] = Variant.From(
                    PropertyUsageFlags.ScriptVariable |
                    PropertyUsageFlags.Default |
                    PropertyUsageFlags.ReadOnly
                )
            },
            new Dictionary {
                ["name"] = nameof(DisplayName),
                ["type"] = Variant.From(Variant.Type.String),
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary {
                ["name"] = nameof(Icon),
                ["type"] = Variant.From(Variant.Type.Object),
                ["hint"] = Variant.From(PropertyHint.ResourceType),
                ["hint_string"] = "Texture2D",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary {
                ["name"] = nameof(TickTimeout),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Range),
                ["hint_string"] = $"0, {ushort.MaxValue.ToString()}, 1",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary {
                ["name"] = nameof(TotalTicks),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Range),
                ["hint_string"] = $"1, {short.MaxValue.ToString()}, 1",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary {
                ["name"] = nameof(ApplicationChance),
                ["type"] = Variant.From(Variant.Type.Float),
                ["hint"] = Variant.From(PropertyHint.Range),
                ["hint_string"] = "0, 1, 0.01",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary {
                ["name"] = nameof(Flags),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Flags),
                ["hint_string"] = Utils.EnumToHintString<EffectFlags>(),
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary {
                ["name"] = nameof(ExcludingEffects),
                ["type"] = Variant.From(Variant.Type.Array),
                ["hint"] = Variant.From(PropertyHint.TypeString),
                ["hint_string"] =
                    $"{Variant.Type.StringName:D}/{PropertyHint.EnumSuggestion:D}:{ResourceDB.EffectIdsHintString}",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary {
                ["name"] = nameof(Behavior),
                ["type"] = Variant.From(Variant.Type.Object),
                ["hint"] = Variant.From(PropertyHint.ResourceType),
                ["hint_string"] = nameof(EffectBehavior),
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            }
        ];
    }

    public override Variant _PropertyGetRevert(StringName pProperty) {
        if (pProperty == new StringName(nameof(Flags))) {
            return Variant.From(EffectFlags.Instant);
        }

        return default;
    }

    public override bool _PropertyCanRevert(StringName pProperty) {
        return pProperty == new StringName(nameof(Flags));
    }
}