using RPG.scripts.inventory.components;

namespace RPG.scripts.inventory;

public static class GizmoExtensions {
    
    public static ulong GetRemainingCooldown(this Gizmo pGizmo) {
        var chainSpellComponent = pGizmo.GetComponent<ChainSpellComponent>();

        if (chainSpellComponent is not null) {
            return chainSpellComponent.GetRemainingCooldown();
        } 
        
        var spellComponent = pGizmo.GetComponent<SpellComponent>();

        if (spellComponent is not null) {
            return spellComponent.GetRemainingCooldown();
        }

        return 0;
    }

}