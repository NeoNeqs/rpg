using System;
using System.Linq;

namespace RPG.global.enums;

[Flags]
public enum SpellFlags : ulong {
    RequiresEnemyTarget = 1 << 0,
    InternalIsAoe = 1 << 2,
    InternalIsChanneled = 1 << 3
}

public static class SpellFlagsExtensions {
    public static string ToHintString(this SpellFlags pSpellFlags) {
        return string.Join(',',
            Enum.GetNames<SpellFlags>().Where(pName => !pName.StartsWith("Internal")).ToArray());
    }
}