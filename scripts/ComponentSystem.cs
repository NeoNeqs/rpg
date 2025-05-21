using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace RPG;

public partial class ComponentSystem<[MustBeVariant] TComponent> : Resource where TComponent : GodotObject {
    // Components are stored in a Dictionary, but displayed as an Array in the Inspector.
    // See also `_GetPropertyList()`, `_Set()` and `_Get()` below.

    protected Godot.Collections.Dictionary<string, TComponent?> Components = [];
    protected readonly StringName _componentsExportName = new("components");

    public T? GetComponent<T>() where T : TComponent {
        if (Components.TryGetValue(typeof(T).Name, out TComponent? value)) {
            return (T?)value;
        }

        return null;
    }


    public T? GetComponent<T>(T pT) where T : TComponent {
        if (Components.TryGetValue(pT.GetType().Name, out TComponent? value)) {
            return (T?)value;
        }

        return null;
    }

    public bool HasComponent<T>() where T : TComponent {
        return Components.ContainsKey(typeof(T).Name);
    }

    public override Array<Dictionary> _GetPropertyList() {
        return [
            // This is what the Inspector sees
            new Dictionary {
                ["name"] = _componentsExportName,
                ["type"] = (long)Variant.Type.Array,
                ["hint"] = (long)PropertyHint.TypeString,
                ["hint_string"] = $"24/17:{nameof(TComponent)}",
                ["usage"] = (long)PropertyUsageFlags.Editor
            },
            // This is what is stored in a file
            new Dictionary {
                ["name"] = nameof(Components),
                ["type"] = (long)Variant.Type.Dictionary,
                ["usage"] = (long)PropertyUsageFlags.NoEditor | (long)PropertyUsageFlags.ScriptVariable
            },
        ];
    }

    public override Variant _Get(StringName pProperty) {
        if (pProperty == _componentsExportName) {
            // TODO: check if this works
            return Variant.From(Components.Values);
        }

        return default;
    }

    public override bool _Set(StringName pRoperty, Variant pValue) {
        if (pRoperty != _componentsExportName) {
            return false;
        }

        Godot.Collections.Dictionary<string, TComponent?> newComponents = new();
        var currentComponents = pValue.As<Array<TComponent>>();

        foreach (TComponent component in currentComponents) {
            var script = component.GetScript().As<Script?>();

            if (script is null) {
                //Logger.Inventory.Critical($"{component.GetType().Name} has no script attached ???");
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
            newComponents[key] = Components.GetValueOrDefault(key, component);
        }

        Components = newComponents;
        return true;
    }
}