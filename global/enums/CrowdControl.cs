using System;

namespace RPG.global.enums;

[Flags]
public enum CrowdControl : ulong {
    Polymorph = 1 << 0,
    Stun = 1 << 1,
    Root = 1 << 2,
}