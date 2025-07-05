using RPG.global;
using Godot;
using Godot.Collections;
using RPG.global.enums;

namespace RPG.scripts.effects;

[Tool, GlobalClass]
public partial class StatusEffect : Effect {
    public CrowdControl CrowdControlImmunity;
    public CrowdControl CrowdControl;

    public override Array<Dictionary> _GetPropertyList() {
        Array<Dictionary> properties = base._GetPropertyList();

        properties.AddRange([
            new Dictionary() {
                ["name"] = nameof(CrowdControlImmunity),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Flags),
                ["hint_string"] = Utils.EnumToHintString<CrowdControl>(),
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new Dictionary() {
                ["name"] = nameof(CrowdControl),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Flags),
                ["hint_string"] = Utils.EnumToHintString<CrowdControl>(),
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
        ]);

        return properties;
    }
}