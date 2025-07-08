using System;
using System.Reflection;

namespace RPG.global;

public static class Utils {
    public static string HumanizeBytes(ulong pByteCount) {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];
        if (pByteCount == 0) {
            return "0 B";
        }

        int place = Convert.ToInt32(Math.Floor(Math.Log(pByteCount, 1024)));
        double num = pByteCount / Math.Pow(1024, place);
        return $"{num:0.##} {suffixes[place]}";
    }

    public static string HumanizeMicroseconds(ulong pMicroseconds) {
        return pMicroseconds switch {
            < 1000 => $"{pMicroseconds} µs",
            < 1_000_000 => $"{pMicroseconds / 1000.0:0.##} ms",
            _ => $"{pMicroseconds / 1_000_000.0:0.##} s"
        };
    }

    public static string EnumToHintString<TEnum>() where TEnum : struct, Enum {
        return string.Join(',', Array.ConvertAll(Enum.GetNames<TEnum>(), pItem => pItem.ToString()));
    }

    public static void SetProperty<T>(this object pInstance, string pPropertyName, T pNewValue) where T : class {
        Type? type = pInstance.GetType();
        PropertyInfo? property = null;

        while (type != null) {
            property = type.GetProperty(pPropertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property != null && property.CanWrite && property.SetMethod != null) {
                break;
            }

            type = type.BaseType;
        }

        if (property == null) {
            Logger.Core.Error(
                $"Property '{pPropertyName}' not found on type '{pInstance.GetType().Name}' or its base types.");
            return;
        }

        property.SetValue(pInstance, pNewValue);
    }

    public static T? GetProperty<T>(this object pInstance, string pPropertyName) where T : class {
        Type? type = pInstance.GetType();
        PropertyInfo? property = null;

        while (type != null) {
            property = type.GetProperty(pPropertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property != null && property.CanWrite && property.SetMethod != null) {
                break;
            }

            type = type.BaseType;
        }

        if (property == null) {
            Logger.Core.Error(
                $"Property '{pPropertyName}' not found on type '{pInstance.GetType().Name}' or its base types.");
            return null;
        }

        object? value = property.GetValue(pInstance);
        if (value is null) {
            Logger.Core.Error($"Property '{pPropertyName}' is null.");
        }

        return (T?)value;
    }
}