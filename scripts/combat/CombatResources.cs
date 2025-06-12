using Godot;

namespace RPG.scripts.combat;

[GlobalClass]
public partial class CombatResources : Resource {
    [Signal]
    public delegate void DiedEventHandler();
    
    private CombatSystem _combatSystem = null!;

    private long _currentHealth;
    private long _maxHealth;

    public void Initialize(CombatSystem pCombatSystem) {
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