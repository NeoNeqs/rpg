using Godot;
using Godot.Collections;
using RPG.global.enums;

namespace RPG.scripts;

[Tool, GlobalClass]
public partial class Faction : Resource, INamedIdentifiable {
    public Dictionary<StringName, ulong> Reputations { private set; get; } = [];
    public StringName Id { private set; get; } = new();
    public string DisplayName { private set; get; } = string.Empty;

    public bool HasReputation(StringName pOtherFactionId) {
        return Reputations.ContainsKey(pOtherFactionId);
    }

    public bool IsHatedNoCheck(StringName pOtherFactionId) {
        ulong reputation = Reputations[pOtherFactionId];

        return reputation <= (ulong)Reputation.Hated;
    }

    public bool IsNeutralNoCheck(StringName pOtherFactionId) {
        ulong reputation = Reputations[pOtherFactionId];

        return reputation is > (ulong)Reputation.Hated and <= (ulong)Reputation.Neutral;
    }

    public bool IsEnemyNoCheck(StringName pOtherFactionId) {
        ulong reputation = Reputations[pOtherFactionId];

        return reputation <= (ulong)Reputation.Neutral;
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
            }
        ];
    }
}