using System.Collections.Generic;
using Godot;
using RPG.scripts.inventory.components;

namespace RPG.scripts.inventory;

[Tool, GlobalClass]
public partial class Gizmo : ComponentSystem<GizmoComponent>, INamedIdentifiable {
    // IMPORTANT: Properties must have private setters to prevent accidental (or not) modifications!
    [Export] public StringName Id { private set; get; } = new("");

    [Export] public string DisplayName { private set; get; } = "";

    [Export] public Texture2D Icon { private set; get; } = null!;

    [Export(PropertyHint.Range, "1, 100, 1")] public int StackSize { private set; get; } = 1;

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
}