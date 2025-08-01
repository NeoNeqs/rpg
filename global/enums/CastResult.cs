namespace RPG.global.enums;

public enum CastResult {
    Ok = 0,
    NoSpellComponent = 1,
    OnCooldown = 2,
    WrongTarget = 3,
    TargetingError = 4
}