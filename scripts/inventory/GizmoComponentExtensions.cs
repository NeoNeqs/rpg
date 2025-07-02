using Godot;
using RPG.scripts.inventory.components;

namespace RPG.scripts.inventory;

public static class GizmoComponentExtensions {
    public static Color GetRarityColor(this Gizmo pGizmo) {
        var itemComponent = pGizmo.GetComponent<ItemComponent>();

        if (itemComponent is null) {
            return Colors.Black;
        }

        return itemComponent.GetRarityColor();
    }

    public static string GetCurrentDisplayName(this Gizmo pGizmo) {
        var chainSpellComponent = pGizmo.GetComponent<ChainSpellComponent>();

        Gizmo? currentSpell = chainSpellComponent?.GetCurrentSpell();
        if (currentSpell is not null) {
            return currentSpell.DisplayName;
        }

        return pGizmo.DisplayName;
    }


    public static Texture2D GetCurrentIcon(this Gizmo pGizmo) {
        var chainSpellComponent = pGizmo.GetComponent<ChainSpellComponent>();

        Gizmo? currentSpell = chainSpellComponent?.GetCurrentSpell();
        if (currentSpell is not null) {
            return currentSpell.Icon;
        }

        return pGizmo.Icon;
    }

    public static ulong GetCooldown(this Gizmo pGizmo) {
        SpellComponent? spellComponent = pGizmo.GetComponent<SpellComponent, ChainSpellComponent>();

        if (spellComponent is not null) {
            return spellComponent.CooldownSeconds;
        }

        return 0;
    }
}