using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using RPG.global.enums;
using Array = System.Array;

namespace RPG.scripts.combat;

// IMPORTANT: Careful with adding numeric properties (Integer / Decimal) here. See `_Set()` definition down below.

[Tool, GlobalClass]
public partial class Stats : Resource {
    [Signal]
    public delegate void DecimalStatChangedEventHandler(DecimalStat pStat, float pDelta);

    [Signal]
    public delegate void IntegerStatChangedEventHandler(IntegerStat pStat, long pDelta);

    private Godot.Collections.Dictionary<string, float> _decimalData = new();

    private Godot.Collections.Dictionary<string, long> _integerData = new();


    // ReSharper disable once InconsistentNaming
    private Godot.Collections.Dictionary<string, long> _IntegerStats {
        set {
            _integerData = value;
#if TOOLS
            NotifyPropertyListChanged();
#endif
        }
        get => _integerData;
    }

    // ReSharper disable once InconsistentNaming
    private Godot.Collections.Dictionary<string, float> _DecimalStats {
        set {
            _decimalData = value;
#if TOOLS
            NotifyPropertyListChanged();
#endif
        }
        get => _decimalData;
    }

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
        _integerData[key] = pValue;

        EmitSignalIntegerStatChanged(pStat, pValue - oldValue);
    }

    public void SetDecimalStat(DecimalStat pStat, float pValue) {
        string key = pStat.ToString();
        _decimalData.TryGetValue(key, out float oldValue);
        _decimalData[key] = pValue;

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
    }

    public static DecimalStat[] GetDecimalStats() {
        return Enum.GetValues<DecimalStat>();
    }

    public override Array<Dictionary> _GetPropertyList() {
        Array<Dictionary> propertyList = [
#if TOOLS
            // Helper properties for the Inspector that allow adding / removing stats.
            new() {
                ["name"] = nameof(AddInteger),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Enum),
                ["hint_string"] = string.Join(',',
                    Array.ConvertAll(Enum.GetNames<IntegerStat>(), pItem => pItem.ToString())),
                ["usage"] = Variant.From(PropertyUsageFlags.Editor)
            },
            new() {
                ["name"] = nameof(AddDecimal),
                ["type"] = Variant.From(Variant.Type.Int),
                ["hint"] = Variant.From(PropertyHint.Enum),
                ["hint_string"] = string.Join(',',
                    Array.ConvertAll(Enum.GetNames<DecimalStat>(), pItem => pItem.ToString())),
                ["usage"] = Variant.From(PropertyUsageFlags.Editor)
            },
#endif
            // Actual storage of stats:
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
        // Helper properties. Those basically display the contents of the dictionaries as normal properties. Only used in editor.
#if TOOLS
        foreach (KeyValuePair<string, long> attribute in _integerData) {
            propertyList.Add(
                new Dictionary {
                    ["name"] = attribute.Key,
                    ["type"] = Variant.From(Variant.Type.Int),
                    ["usage"] = Variant.From(PropertyUsageFlags.Editor)
                }
            );
        }

        foreach (KeyValuePair<string, float> attribute in _decimalData) {
            propertyList.Add(
                new Dictionary {
                    ["name"] = attribute.Key,
                    ["type"] = Variant.From(Variant.Type.Float),
                    ["usage"] = Variant.From(PropertyUsageFlags.Editor)
                }
            );
        }
#endif
        return propertyList;
    }

    /// Redirects storage of Integer / Decimal properties to Dictionaries `_integerData` / `_decimalData` respectively.
    public override bool _Set(StringName pProperty, Variant pValue) {
        string propertyName = pProperty.ToString();

        // Currently ALL Integer / Decimal properties of this class are handled through here!

        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        // Only care about ints and floats.
        switch (pValue.VariantType) {
            case Variant.Type.Int: {
                long valueAsLong = pValue.As<long>();

                // This is where and how properties are removed (no longer stored).
                // If editor encounters one of the stats and sees an "impossible" value it removes it from respective dictionary:
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
    // AddInteger and AddDecimal are helper properties in the Inspector that just add stats to the resource.
    private IntegerStat AddInteger {
        set {
            if (_integerData.TryAdd(value.ToString(), 1)) {
                NotifyPropertyListChanged();
            }
        }
    }

    private DecimalStat AddDecimal {
        set {
            if (_decimalData.TryAdd(value.ToString(), 1.0f)) {
                NotifyPropertyListChanged();
            }
        }
    }
#endif

#if TOOLS
    public override bool _PropertyCanRevert(StringName pProperty) {
        return _integerData.TryGetValue(pProperty.ToString(), out long _) ||
               _decimalData.TryGetValue(pProperty.ToString(), out float _);
    }

    public override Variant _PropertyGetRevert(StringName pProperty) {
        if (_integerData.TryGetValue(pProperty.ToString(), out long _)) {
            return Variant.From(long.MaxValue); // Default revert (impossible) value of Integer Stats
        }

        if (_decimalData.TryGetValue(pProperty.ToString(), out float _)) {
            return Variant.From(Mathf.Inf); // Default revert (impossible) value of Decimal Stats
        }

        return default;
    }

    // A neat trick to do compile time asserts, although the thrown error message is not very useful.
    // Source: https://www.lunesu.com/archives/62-Static-assert-in-C!.html
    // ReSharper disable once UnusedType.Local

    private sealed class StaticAssert {
        // IMPORTANT: Changing those names will break existing resources!
#pragma warning disable CS0414 // Field is assigned but its value is never used
        // ReSharper disable HeuristicUnreachableCode
        private byte _assert1 = nameof(_DecimalStats) == "_DecimalStats" ? 0 : -1;
        private byte _assert2 = nameof(_IntegerStats) == "_IntegerStats" ? 0 : -1;
        // ReSharper restore HeuristicUnreachableCode
#pragma warning restore CS0414 // Field is assigned but its value is never used
    }
#endif
}