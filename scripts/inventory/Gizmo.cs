using Godot;
using Godot.Collections;
using RPG.Global;
using RPG.scripts.components;
using CollectionExtensions = System.Collections.Generic.CollectionExtensions;

namespace RPG.scripts.inventory;

[GlobalClass]
public partial class Gizmo : Resource {
    [Export] public required string DisplayName;
    [Export] public int StackSize;

    // Components are stored in a Dictionary, but displayed as an Array in the Inspector.
    // See also `_GetPropertyList()`, `_Set()` and `_Get()` below.
    
    private Dictionary<string, GizmoComponent?> _components = new();

    private readonly StringName _componentsExportName = new("components");

    public T? GetComponent<T>() where T : GizmoComponent {
        if (_components.TryGetValue(typeof(T).Name, out GizmoComponent? value)) {
            return (T?)value;
        }

        return null;
    }
    
    public T? GetComponent<T>(T pT) where T : GizmoComponent {
        if (_components.TryGetValue(pT.GetType().Name, out GizmoComponent? value)) {
            return (T?)value;
        }

        return null;
    }

    // Move this to CombatSystem
    public void Use() {
        var spellComponent = GetComponent<SpellComponent>();

        if (spellComponent is not null) {
            spellComponent.Cast();
        }
    }
    
    public override Array<Dictionary> _GetPropertyList() {
        return [
            // This is what the Inspector sees
            new Dictionary {
                ["name"] = _componentsExportName,
                ["type"] = (long)Variant.Type.Array,
                ["hint"] = (long)PropertyHint.TypeString,
                ["hint_string"] = $"24/17:{nameof(GizmoComponent)}",
                ["usage"] = (long)PropertyUsageFlags.Editor
            },
            // This is what is stored in a file
            new Dictionary {
                ["name"] = nameof(_components),
                ["type"] = (long)Variant.Type.Dictionary,
                ["usage"] = (long)PropertyUsageFlags.NoEditor | (long)PropertyUsageFlags.ScriptVariable
            },
        ];
    }

    public override Variant _Get(StringName pProperty) {
        if (pProperty == _componentsExportName) {
            // TODO: check if this works
            return Variant.From(_components.Values);
        }

        return default;
    }

    public override bool _Set(StringName pRoperty, Variant pValue) {
        if (pRoperty != _componentsExportName) {
            return false;
        }

        Dictionary<string, GizmoComponent?> newComponents = new();
        var currentComponents = pValue.As<Array<GizmoComponent>>();

        foreach (GizmoComponent component in currentComponents) {
            var script = component.GetScript().As<Script?>();

            if (script is null) {
                Logger.Inventory.Critical($"{component.GetType().Name} has no script attached ???");
                continue;
            }

            // TODO: Change this to `component.GetType().IsAbstract`
            
            // Disallow instantiation of the abstract GizmoComponent class
            if (script.GetBaseScript() is null) {
                newComponents["null"] = null;
                continue;
            }

            // TODO: Change this to `component.GetType().Name`
            var key = script.GetGlobalName().ToString();
            // Attempt to extract old value from _components, otherwise set the new one
            newComponents[key] = CollectionExtensions.GetValueOrDefault(_components, key, component);
        }

        _components = newComponents;
        return true;
    }
}