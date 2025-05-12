using System;

namespace RPG.global;

public static class Utils {
    public static string HumanizeBytes(long pByteCount) {
        string[] suffixes = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];
        if (pByteCount == 0) {
            return "0 B";
        }

        var place = Convert.ToInt32(Math.Floor(Math.Log(pByteCount, 1024)));
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
}