using Godot;
using RPG.scripts.inventory.components;

namespace RPG.scripts.inventory;

public static class GizmoComponentExtensions {
    public static Color GetRarityColor(this Gizmo pGizmo) {
        ItemComponent? itemComponent = pGizmo.GetComponent<ItemComponent>();

        if (itemComponent is null) {
            return Colors.Black;
        }
        
        return itemComponent.GetRarityColor();
    }
    
    public static ulong GetCooldown(this Gizmo pGizmo) {
        ChainSpellComponent? chainSpellComponent = pGizmo.GetComponent<ChainSpellComponent>();

        if (chainSpellComponent != null) {
            return chainSpellComponent.CooldownSeconds;
        }

        SpellComponent? spellComponent = pGizmo.GetComponent<SpellComponent>();

        if (spellComponent != null) {
            return spellComponent.CooldownSeconds;
        }

        return 0;
    }
}