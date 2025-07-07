using Godot;
using Godot.Collections;
using RPG.scripts.effects;

namespace RPG.scripts.inventory.components;

[Tool, GlobalClass]
public sealed partial class ChainSpellComponent : SpellComponent {
    [Export] public Gizmo[] Spells = [];

    private int _current = -1;

    public override void Cast(Gizmo pSource) {
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

    public override bool IsAoe() {
        if (_current == -1) {
            return base.IsAoe();
        }

        return Spells[_current].GetComponent<SpellComponent>()!.IsAoe();
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

    public override ushort GetRange() {
        if (_current == -1) {
            return base.GetRange();
        }

        return Spells[_current].GetComponent<SpellComponent>()!.GetRange();
    }
}