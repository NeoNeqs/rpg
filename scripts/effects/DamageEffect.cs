using RPG.global;
using Godot;
using Godot.Collections;
using RPG.global.enums;
using RPG.scripts.combat;

namespace RPG.scripts.effects;

[Tool, GlobalClass]
public sealed partial class DamageEffect : StackingEffect {
    public DamageType DamageType { private set; get; }

    public float GetTotalDamage(Stats pAttackerStats) {
        return FlatValue + (pAttackerStats.GetIntegerStat(StatScale) * StatScaleCoefficient);
    }

    public override Array<Dictionary> _GetPropertyList() {
        Array<Dictionary> properties = base._GetPropertyList();

        properties.AddRange([
            new Dictionary() {
                ["name"] = nameof(DamageType),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Enum),
                ["hint_string"] = Utils.EnumToHintString<DamageType>(),
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            }
        ]);

        return properties;
    }
}