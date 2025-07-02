using Godot;

namespace RPG.scripts.combat;

/// <summary>
/// Tracks per entity resources like Health, Mana, etc.
/// </summary>
public partial class CombatResources : RefCounted {
    [Signal]
    public delegate void DiedEventHandler();

    
    private readonly CombatSystem _combatSystem = null!;

    private long _currentHealth;

    public CombatResources(CombatSystem pCombatSystem) {
        _combatSystem = pCombatSystem;
        _currentHealth = _combatSystem.GetMaxHealth();
    }

    public CombatResources() { }

    public void ModifyHealth(double pDamage) {
        long maxHealth = _combatSystem.GetMaxHealth();
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