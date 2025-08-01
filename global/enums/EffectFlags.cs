using System;

namespace RPG.global.enums;

[Flags]
public enum EffectFlags : ulong {
    // IMPORTANT: There must not be holes between these value. Godot expects the next value to always be the next power of 2.
    NeedsEnemyTarget = 1 << 0,
    NeedsFriendlyTarget = 1 << 1,
    Instant = 1 << 2,
}