using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using RPG.world;

namespace RPG.scripts.combat;

public partial class Attributes : Resource {
    [Signal]
    public delegate void ValueChangedEventHandler(string pPropertyName, long pDelta);

    [Export]
    private long Stamina {
        get => _stamina;
        set => SetField(ref _stamina, value);
    }

    [Export]
    private long Strength {
        get => _strength;
        set => SetField(ref _strength, value);
    }

    [Export]
    private long Armor {
        get => _armor;
        set => SetField(ref _armor, value);
    }

    private long _stamina;
    private long _strength;
    private long _armor;

    public void Set(string pPropertyName, long pValue) {
        var propertyNameSN = new StringName(pPropertyName);
        var currentValue = Get(propertyNameSN).As<long>();
        
        Set(propertyNameSN, Variant.From(currentValue + pValue));
    }

    
    public bool IsConnectedToValueChanged(Action<string, long> pAction) {
        return IsConnected(SignalName.ValueChanged, Callable.From(pAction));
    }

    public IEnumerable<string> GetPropertyNames() {
        foreach (PropertyInfo info in GetType().GetProperties(BindingFlags.Public)) {
            yield return info.Name;
        }
    }

    private void SetField(ref long pField, long pValue, [CallerMemberName] string? pPropertyName = null) {
        if (EqualityComparer<long>.Default.Equals(pField, pValue)) {
            return;
        }

        EmitSignalValueChanged(pPropertyName, pValue - pField);
        pField = pValue;
    }
}