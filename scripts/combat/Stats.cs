using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Godot;
using Godot.Collections;

namespace RPG.scripts.combat;

[Tool, GlobalClass]
public partial class Stats : Resource {
    [Signal]
    public delegate void IntegerStatChangedEventHandler(IntegerStat pStat, long pDelta);

    [Signal]
    public delegate void DecimalStatChangedEventHandler(DecimalStat pStat, float pDelta);

    public enum IntegerStat {
        Strength,
        Stamina,
        Intelligence,
        Spirit,
        Armor,
        ShadowResistance,
        NatureResistance,
        ArmorPenetration,
        ShadowPenetration,
        NaturePenetration,
    }

    public enum DecimalStat {
        StrengthMultiplayer,
        StaminaMultiplayer,
    }

    // ReSharper disable once InconsistentNaming
    private Godot.Collections.Dictionary<string, long> _IntegerStats {
        set {
            _integerData = value;
            NotifyPropertyListChanged();
        }
        get => _integerData;
    }

    private Godot.Collections.Dictionary<string, long> _integerData = new();

    // ReSharper disable once InconsistentNaming
    private Godot.Collections.Dictionary<string, float> _DecimalStats {
        set {
            _decimalData = value;
            NotifyPropertyListChanged();
        }
        get => _decimalData;
    }

    private Godot.Collections.Dictionary<string, float> _decimalData = new();

#if TOOLS
    // A neat trick to do compile time asserts, although the thrown error message is not very useful.
    // Source: https://www.lunesu.com/archives/62-Static-assert-in-C!.html
    [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
    [SuppressMessage("ReSharper", "UnusedType.Local")]
    private sealed class StaticAssert {
        // Changing those names will break existing resources!
#pragma warning disable CS0414 // Field is assigned but its value is never used
        private byte _assert1 = nameof(_DecimalStats) == "_DecimalStats" ? 0 : -1;
        private byte _assert2 = nameof(_IntegerStats) == "_IntegerStats" ? 0 : -1;
#pragma warning restore CS0414 // Field is assigned but its value is never used
    }

    private IntegerStat AddInteger {
        set {
            _integerData.TryAdd(value.ToString(), 1);
            NotifyPropertyListChanged();
        }
    }

    private DecimalStat AddDecimal {
        set {
            _decimalData.TryAdd(value.ToString(), 1.0f);
            NotifyPropertyListChanged();
        }
    }
#endif

    public long GetIntegerStat(IntegerStat pStat, long pDefault = 0) {
        string key = pStat.ToString();
        _integerData.TryAdd(key, pDefault);

        return _integerData[key];
    }

    public float GetDecimalStat(DecimalStat pStat, float pDefault = 0) {
        string key = pStat.ToString();
        _decimalData.TryAdd(key, pDefault);

        return _decimalData[key];
    }

    public void SetIntegerStat(IntegerStat pStat, long pValue) {
        string key = pStat.ToString();
        _integerData.TryGetValue(key, out long oldValue);
        _integerData.TryAdd(key, pValue);
        
        EmitSignalIntegerStatChanged(pStat, pValue - oldValue);
    }

    public void SetDecimalStat(DecimalStat pStat, float pValue) {
        string key = pStat.ToString();
        _decimalData.TryGetValue(key, out float oldValue);
        _decimalData.TryAdd(key, pValue);

        EmitSignalDecimalStatChanged(pStat, pValue - oldValue);
    }


    public bool IsConnectedToIntegerStatChanged(Action<IntegerStat, long> pAction) {
        return IsConnected(SignalName.IntegerStatChanged, Callable.From(pAction));
    }

    public bool IsConnectedToDecimalStatChanged(Action<DecimalStat, float> pAction) {
        return IsConnected(SignalName.DecimalStatChanged, Callable.From(pAction));
    }

    public static IntegerStat[] GetIntegerStats() {
        return Enum.GetValues<IntegerStat>();
        // return Enum.GetNames<IntegerStat>();
    }

    public static DecimalStat[] GetDecimalStats() {
        return Enum.GetValues<DecimalStat>();
        // return Enum.GetNames<DecimalStat>();
    }

    public override Array<Dictionary> _GetPropertyList() {
        Array<Dictionary> result = [
#if TOOLS
            new() {
                ["name"] = nameof(AddInteger),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Enum),
                ["hint_string"] = string.Join(',',
                    System.Array.ConvertAll(Enum.GetNames<IntegerStat>(), pItem => pItem.ToString())),
                ["usage"] = Variant.From(PropertyUsageFlags.Editor)
            },
            new() {
                ["name"] = nameof(AddDecimal),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Enum),
                ["hint_string"] = string.Join(',',
                    System.Array.ConvertAll(Enum.GetNames<DecimalStat>(), pItem => pItem.ToString())),
                ["usage"] = Variant.From(PropertyUsageFlags.Editor)
            },
#endif
            new() {
                ["name"] = nameof(_IntegerStats),
                ["type"] = Variant.From(Variant.Type.Dictionary),
                ["usage"] = Variant.From(PropertyUsageFlags.NoEditor | PropertyUsageFlags.Storage)
            },
            new() {
                ["name"] = nameof(_DecimalStats),
                ["type"] = Variant.From(Variant.Type.Dictionary),
                ["usage"] = Variant.From(PropertyUsageFlags.NoEditor | PropertyUsageFlags.Storage)
            }
        ];
#if TOOLS
        foreach (KeyValuePair<string, long> attribute in _integerData) {
            result.Add(
                new Dictionary {
                    ["name"] = attribute.Key,
                    ["type"] = Variant.From(Variant.Type.Int),
                    ["usage"] = Variant.From(PropertyUsageFlags.Editor)
                }
            );
        }

        foreach (KeyValuePair<string, float> attribute in _decimalData) {
            result.Add(
                new Dictionary {
                    ["name"] = attribute.Key,
                    ["type"] = Variant.From(Variant.Type.Float),
                    ["usage"] = Variant.From(PropertyUsageFlags.Editor)
                }
            );
        }
#endif
        return result;
    }

    public override bool _Set(StringName pProperty, Variant pValue) {
        string propertyName = pProperty.ToString();

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (pValue.VariantType) {
            case Variant.Type.Int: {
                long valueAsLong = pValue.As<long>();
#if TOOLS
                if (valueAsLong == long.MaxValue) {
                    _integerData.Remove(propertyName);
                    NotifyPropertyListChanged();
                    return true;
                }
#endif
                _integerData[propertyName] = valueAsLong;
                break;
            }
            case Variant.Type.Float: {
                float valueAsLong = pValue.As<float>();
#if TOOLS
                if (float.IsPositiveInfinity(valueAsLong)) {
                    _decimalData.Remove(propertyName);
                    NotifyPropertyListChanged();
                    return true;
                }
#endif
                _decimalData[propertyName] = valueAsLong;
                break;
            }
        }

        return false;
    }

    public override Variant _Get(StringName pProperty) {
        if (_integerData.TryGetValue(pProperty.ToString(), out long intValue)) {
            return Variant.From(intValue);
        }

        if (_decimalData.TryGetValue(pProperty.ToString(), out float decimalValue)) {
            return Variant.From(decimalValue);
        }

        return default;
    }


#if TOOLS
    public override bool _PropertyCanRevert(StringName pProperty) {
        return _integerData.TryGetValue(pProperty.ToString(), out long _) ||
               _decimalData.TryGetValue(pProperty.ToString(), out float _);
    }

    public override Variant _PropertyGetRevert(StringName pProperty) {
        if (_integerData.TryGetValue(pProperty.ToString(), out long _)) {
            return long.MaxValue;
        }

        if (_decimalData.TryGetValue(pProperty.ToString(), out float _)) {
            return Mathf.Inf;
        }

        return default;
    }
#endif
}