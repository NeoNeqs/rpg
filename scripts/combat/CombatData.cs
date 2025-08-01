using Godot;
using RPG.global.enums;

namespace RPG.scripts.combat;

/// <summary>
///     Tracks per entity resources like Health, Mana, etc.
/// </summary>
public partial class CombatData : RefCounted {
    [Signal]
    public delegate void CrowdControlAppliedEventHandler(CrowdControl pCrowdControl);

    [Signal]
    public delegate void DiedEventHandler();

    private readonly StatCalculator _statCalculator = null!;

    private long _currentHealth;
    public CrowdControl AppliedCrowdControl;

    public CrowdControl CrowdControlImmunity;

    public CombatData(StatCalculator pStatCalculator) {
        _statCalculator = pStatCalculator;
        _currentHealth = _statCalculator.GetMaxHealth();
    }

    // Parameterless constructor is required for the Godot to initialize a script/game object
    // ReSharper disable once UnusedMember.Global
    public CombatData() { }

    public void ApplyCrowdControl(CrowdControl pCrowdControl) {
        // Prevent CC if Entity is immune
        pCrowdControl &= ~CrowdControlImmunity;
        AppliedCrowdControl |= pCrowdControl;
        EmitSignalCrowdControlApplied(AppliedCrowdControl);
    }

    // TODO: Test if it works
    public void RemoveCrowdControl(CrowdControl pCrowdControl) {
        AppliedCrowdControl &= ~pCrowdControl;
        EmitSignalCrowdControlApplied(AppliedCrowdControl);
    }

    public bool IsImmuneTo(CrowdControl pCrowdControl) {
        return CrowdControlImmunity.HasFlag(pCrowdControl);
    }

    public bool HasApplied(CrowdControl pCrowdControl) {
        return AppliedCrowdControl.HasFlag(pCrowdControl);
    }

    public void ModifyHealth(double pDamage) {
        long maxHealth = _statCalculator.GetMaxHealth();
        long testHealth = _currentHealth + (long)pDamage;

        if (testHealth > maxHealth) {
            _currentHealth = maxHealth;
        } else if (testHealth < 0) {
            _currentHealth = 0;
            EmitSignalDied();
        } else {
            _currentHealth = testHealth;
        }
    }
}