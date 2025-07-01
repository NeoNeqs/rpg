using RPG.global;
using Godot;
using Godot.Collections;

namespace RPG.scripts.inventory.components;

[Tool, GlobalClass]
public sealed partial class ChainSpellComponent : SpellComponent {
    [Export] public Array<Gizmo> Spells = [];

    private int _current = -1;

    // TODO: make private
    public void Chain() {
        _current = Mathf.Wrap(_current + 1, -1, Spells.Count);
    }

    public override CastResult Cast(Gizmo pSource) {
        Gizmo currentSpell = GetCurrentSpell() ?? pSource;

        if (IsOnCooldown()) {
            return CastResult.OnCooldown;
        }

        Gizmo nextSpell;
        
        if (_current == -1) {
            Chain();
            // Next spell is correct since `Chain()` is called above!

            nextSpell = GetCurrentSpell() ?? pSource;
            EmitCastComplete(nextSpell.GetCooldown() * 1_000_000);
            LastCastTime = Time.GetTicksUsec();
            return CastResult.Ok;
        }

#if TOOLS
        var chainSpellComponent = currentSpell.GetComponent<ChainSpellComponent>();
        if (chainSpellComponent is not null) {
            Logger.Combat.Warn("Chain spells should not chain into another chain spell!", true);
        }
#endif
        
        var spellComponent = currentSpell.GetComponent<SpellComponent>();
        if (spellComponent is null) {
            return CastResult.NoSpellFound;
        }

        spellComponent.Cast(pSource);

        nextSpell = GetCurrentSpell() ?? pSource;
        EmitCastComplete(nextSpell.GetCooldown() * 1_000_000);
        LastCastTime = Time.GetTicksUsec();
        
        return CastResult.Ok;
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