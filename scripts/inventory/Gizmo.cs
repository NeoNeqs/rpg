using System.Collections.Generic;
using Godot;
using Godot.Collections;
using RPG.global;
using RPG.scripts.inventory.components;

namespace RPG.scripts.inventory;

[Tool, GlobalClass]
public partial class Gizmo : ComponentSystem<GizmoComponent> {
    private StringName _id = new("");

    [Export]
    public StringName Id {
        private set {
#if TOOLS
            if (value.IsEmpty) {
                _id = DisplayName.ToSnakeCase();
                return;
            }
#endif
            _id = value;
        }
        get => _id;
    }

    [Export] public string DisplayName { private set; get; } = "";
    [Export] public Texture2D Icon { private set; get; } = null!;
    [Export] public int StackSize { private set; get; } = 1;


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

#if TOOLS
    public override bool _Set(StringName pProperty, Variant pValue) {
        if (pProperty == ComponentsExportName) {
            int counter = 0;
            var components = pValue.As<Array<GizmoComponent?>>();
            foreach (GizmoComponent? component in components) {
                if (component is ChainSpellComponent or SpellComponent) {
                    counter++;
                }
            }

            if (counter >= 2) {
                Logger.Core.Warn(
                    $"Gizmos should not have both {nameof(ChainSpellComponent)} and {nameof(SpellComponent)}.");
            }
        }

        return base._Set(pProperty, pValue);
    }
#endif
}