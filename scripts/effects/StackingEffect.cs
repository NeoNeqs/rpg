using RPG.global;
using Godot;
using Godot.Collections;
using RPG.scripts.combat;

namespace RPG.scripts.effects;

[Tool]
public abstract partial class StackingEffect : Effect {
    public uint FlatValue { private set; get; }
    public Stats.IntegerStat StatScale { private set; get; }
    public float StatScaleCoefficient { private set; get; }

    public ushort MaxStacks { private set; get; } = 1;

    // Always start with minimum stacks!
    public int CurrentStacks { private set; get; } = 1;

    public void Stack() {
        if (CurrentStacks < MaxStacks) {
            CurrentStacks++;
        }

        // Reset the timer since new stack was just applied
        if (Timer?.IsInsideTree() ?? false) {
            Timer.Start();
        }
    }

    public override Array<Dictionary> _GetPropertyList() {
        Array<Dictionary> properties = base._GetPropertyList();

        properties.AddRange([
            new Dictionary {
                ["name"] = nameof(FlatValue),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Range),
                ["hint_string"] = $"0, {uint.MaxValue.ToString()}, 1",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary {
                ["name"] = nameof(StatScale),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Enum),
                ["hint_string"] = Utils.EnumToHintString<Stats.IntegerStat>(),
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary {
                ["name"] = nameof(StatScaleCoefficient),
                ["type"] = Variant.From(Variant.Type.Float),
                ["hint"] = Variant.From(PropertyHint.Range),
                ["hint_string"] = "0, 255, 0.1",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary {
                ["name"] = nameof(MaxStacks),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Range),
                ["hint_string"] = $"0, {ushort.MaxValue.ToString()}, 0.1",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            }
        ]);

        return properties;
    }
}