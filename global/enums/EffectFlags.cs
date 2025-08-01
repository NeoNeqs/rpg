using System;

namespace RPG.global.enums;

[Flags]
public enum EffectFlags : ulong {
    // IMPORTANT: There must not be holes between these value. Godot expects the next value to always be the next power of 2.
    TargetsEnemies = 1 << 0,
    Instant = 1 << 2
}