using Godot;

namespace RPG.scripts.combat;

public partial class CombatResources : RefCounted {
    [Signal]
    public delegate void DiedEventHandler();
    
    private readonly CombatSystem _combatSystem;

    private long _currentHealth;
    private long _maxHealth;

    public CombatResources(CombatSystem pCombatSystem) {
        _combatSystem = pCombatSystem;
        _maxHealth = _combatSystem.GetMaxHealth();
        _currentHealth = _maxHealth;
    }

    public void ModifyHealth(double pDamage) {
        long testHealth = _currentHealth + (long)pDamage;
        if (testHealth > _maxHealth) {
            _currentHealth = _maxHealth;
        } else if (testHealth < 0) {
            _currentHealth = 0;
            EmitSignalDied();
        } else {
            _currentHealth = testHealth;
        }
    }
}