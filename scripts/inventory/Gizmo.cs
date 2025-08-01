using System.Collections.Generic;
using Godot;
using Godot.Collections;
using RPG.scripts.inventory.components;

namespace RPG.scripts.inventory;

[Tool, GlobalClass]
public partial class Gizmo : ComponentSystem<GizmoComponent>, INamedIdentifiable {
    public StringName Id { private set; get; } = new("");

    public string DisplayName { private set; get; } = "";

    public Texture2D? Icon { private set; get; }

    public int StackSize { private set; get; } = 1;

    // I have tried using Godot's `Resource.Duplicate` to no avail. It does not function as I want it to...
    public Gizmo Duplicate() {
        var newGizmo = new Gizmo() {
            Id = Id,
            DisplayName = DisplayName,
            Icon = Icon,
            StackSize = StackSize,
        };

        foreach (KeyValuePair<string, GizmoComponent?> kvp in Components) {
            if (kvp.Value is null) {
                continue;
            }

            newGizmo.Components.Add(kvp.Key, (GizmoComponent?)kvp.Value.Duplicate(false));
        }

        return newGizmo;
    }

    public override Array<Dictionary> _GetPropertyList() {
        Array<Dictionary> properties = [
            new() {
                ["name"] = nameof(Id),
                ["type"] = Variant.From(Variant.Type.StringName),
                ["usage"] = Variant.From(
                    (long)PropertyUsageFlags.ReadOnly |
                    (long)PropertyUsageFlags.Default |
                    (long)PropertyUsageFlags.ScriptVariable
                )
            },
            new() {
                ["name"] = nameof(DisplayName),
                ["type"] = Variant.From(Variant.Type.String),
                ["usage"] = Variant.From(
                    (long)PropertyUsageFlags.Default |
                    (long)PropertyUsageFlags.ScriptVariable
                )
            },
            new() {
                ["name"] = nameof(Icon),
                ["type"] = Variant.From(Variant.Type.Object),
                ["hint"] = Variant.From(PropertyHint.ResourceType),
                ["hint_string"] = "Texture2D",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
            new () {
                ["name"] = nameof(StackSize),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Range),
                ["hint_string"] = "1, 100, 1",
                ["usage"] = Variant.From(PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)
            },
        ];

        properties.AddRange(base._GetPropertyList());

        return properties;
    }
}