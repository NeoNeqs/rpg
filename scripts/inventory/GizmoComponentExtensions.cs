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
        var sequenceSpellComponent = pGizmo.GetComponent<SequenceSpellComponent>();

        Gizmo? currentSpell = sequenceSpellComponent?.GetCurrentSpell();
        if (currentSpell is not null) {
            return currentSpell.DisplayName;
        }

        return pGizmo.DisplayName;
    }

    public static Texture2D? GetCurrentIcon(this Gizmo pGizmo) {
        var sequenceSpellComponent = pGizmo.GetComponent<SequenceSpellComponent>();

        Gizmo? currentGizmo = sequenceSpellComponent?.GetCurrentSpell();
        if (currentGizmo is not null) {
            return currentGizmo.Icon;
        }

        return pGizmo.Icon;
    }
}