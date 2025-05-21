using Godot;

namespace RPG.scripts.combat;

[GlobalClass]
public partial class CombatResources : Resource {
    private CombatSystem _combatSystem = null!;

    private long _currentHealth;
    private long _maxHealth;

    public void Initialize(CombatSystem pCombatSystem) {
        _combatSystem = pCombatSystem;
        _maxHealth = _combatSystem.GetMaxHealth();
        _currentHealth = _maxHealth;
    }

    public void ApplyDamage(double pDamage) {
        _currentHealth -= (long)pDamage;
    }
}