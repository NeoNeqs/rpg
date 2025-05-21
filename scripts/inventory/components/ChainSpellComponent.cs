using System;
using Godot;
using Godot.Collections;

namespace RPG.scripts.inventory.components;

[Tool, GlobalClass]
public sealed partial class ChainSpellComponent : SpellComponent {
    [Export] public Array<Gizmo> Spells = [];

    private int _current = -1;

    public void Chain() {
        _current = Mathf.Wrap(_current + 1, -1, Spells.Count);
    }

    public override bool Cast() {
        if (_current == -1) {
            return base.Cast();
        }

        return false;
    }

    public Gizmo? GetNextSpell() {
        int next = Mathf.Wrap(_current + 1, -1, Spells.Count);
        if (next == -1) {
            return null;
        }

        return Spells[next];
    }

    public Gizmo? GetCurrentSpell() {
        if (_current == -1) {
            return null;
        }

        return Spells[_current];
    }
}