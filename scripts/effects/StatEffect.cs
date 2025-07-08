using RPG.global;
using Godot;
using Godot.Collections;
using RPG.global.enums;

namespace RPG.scripts.effects;

[Tool, GlobalClass]
public sealed partial class StatEffect : StackingEffect {
    public IntegerStat Stat { private set; get; }
    
    public override Array<Dictionary> _GetPropertyList() {
        Array<Dictionary> properties = base._GetPropertyList();

        properties.AddRange([
            new Dictionary() {
                ["name"] = nameof(Stat),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Enum),
                ["hint_string"] = Utils.EnumToHintString<IntegerStat>(),
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            }
        ]);

        return properties;
    }
}