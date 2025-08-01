using Godot;
using RPG.scripts.effects;

namespace RPG.scripts.inventory.components;

// TODO: rename to SequenceSpellComponent
[Tool, GlobalClass]
public sealed partial class SequenceSpellComponent : SpellComponent {
    private int _current = -1;
    [Export] public Gizmo[] Spells = [];

    public override void Cast() {
        if (_current == -1) {
            LastCastTimeMicroseconds = Time.GetTicksUsec();
            float cooldown = Chain();
            EmitSignalCastComplete(cooldown);
        } else {
            var spellComponent = Spells[_current].GetComponent<SpellComponent>();

            if (spellComponent is null) {
                return;
            }

            spellComponent.LastCastTimeMicroseconds = Time.GetTicksUsec();
            float cooldown = Chain();
            EmitSignalCastComplete(cooldown);
        }
    }

    private float Chain() {
        _current = Mathf.Wrap(_current + 1, -1, Spells.Length);

        if (_current == -1) {
            LastCastTimeMicroseconds = Time.GetTicksUsec();
            return CooldownSeconds;
        }

        var sp = Spells[_current].GetComponent<SpellComponent>()!;
        sp.LastCastTimeMicroseconds = Time.GetTicksUsec();
        return sp.CooldownSeconds;
    }

    public Gizmo? GetCurrentSpell() {
        if (_current == -1) {
            return null;
        }

        return Spells[_current];
    }

    public override Effect[] GetEffects() {
        if (_current == -1) {
            return Effects;
        }

        return Spells[_current].GetComponent<SpellComponent>()!.GetEffects();
    }

    public override float GetCooldownSeconds() {
        if (_current == -1) {
            return base.GetCooldownSeconds();
        }

        return Spells[_current].GetComponent<SpellComponent>()!.GetCooldownSeconds();
    }

    public override ushort GetRange() {
        if (_current == -1) {
            return base.GetRange();
        }

        return Spells[_current].GetComponent<SpellComponent>()!.GetRange();
    }

    public override float GetCastTimeSeconds() {
        if (_current == -1) {
            return base.GetRange();
        }

        return Spells[_current].GetComponent<SpellComponent>()!.GetCastTimeSeconds();
    }

    public override Id[] GetLinkedSpells() {
        if (_current == -1) {
            return base.GetLinkedSpells();
        }

        return Spells[_current].GetComponent<SpellComponent>()!.GetLinkedSpells();
    }

    public override bool IsAoe() {
        if (_current == -1) {
            return base.IsAoe();
        }

        return Spells[_current].GetComponent<SpellComponent>()!.IsAoe();
    }

    public override bool RequiresEnemyTarget() {
        if (_current == -1) {
            return base.RequiresEnemyTarget();
        }

        return Spells[_current].GetComponent<SpellComponent>()!.RequiresEnemyTarget();
    }

    public override bool IsOnCooldown() {
        if (_current == -1) {
            return base.IsOnCooldown();
        }

        return Spells[_current].GetComponent<SpellComponent>()!.IsOnCooldown();
    }

    public override float GetRemainingCooldown() {
        if (_current == -1) {
            return base.GetRemainingCooldown();
        }

        return Spells[_current].GetComponent<SpellComponent>()!.GetRemainingCooldown();
    }
}