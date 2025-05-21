using Godot;

namespace RPG.scripts.combat;

public partial class CombatResources : Resource {
    private StatSystem _statSystem = null!;

    private long _currentHealth;
    private long _maxHealth;

    public void Initialize(StatSystem pStatSystem) {
        _statSystem = pStatSystem;
        _maxHealth = _statSystem.GetMaxHealth();
        _currentHealth = _maxHealth;
    }

    public void ApplyDamage(double pDamage) {
        _currentHealth -= (long)pDamage;
    }
}