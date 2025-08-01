namespace RPG.global.enums;

public enum CastResult : byte {
    Ok = 0,
    OkRefreshed = 1,
    EffectIgnored = 2,
    Missed = 3,

    NoSpellComponent = 100,
    OnCooldown = 101,
    SpellWrongTarget = 103,
    NoValidTarget = 104
}

public static class CastResultExtensions {
    public static bool IsError(this CastResult pCastResult) {
        return pCastResult >= CastResult.NoSpellComponent;
    }

    public static bool IsOk(this CastResult pCastResult) {
        return pCastResult < CastResult.NoSpellComponent;
    }
}