using System;
using System.Collections.Generic;
using global::RPG.global;
using Godot;
using Godot.Collections;
using RPG.scripts.effects;
using RPG.scripts.effects.components;
using RPG.scripts.inventory.components;
using Array = Godot.Collections.Array;

namespace RPG.scripts.inventory;

public static class GizmoComponentExtensions {
    public static Color GetRarityColor(this Gizmo pGizmo) {
        var itemComponent = pGizmo.GetComponent<ItemComponent>();

        if (itemComponent is null) {
            return Colors.Black;
        }
        
        return itemComponent.GetRarityColor();
    }
    
    public static ulong GetCooldown(this Gizmo pGizmo) {
        var chainSpellComponent = pGizmo.GetComponent<ChainSpellComponent>();

        if (chainSpellComponent != null) {
            return chainSpellComponent.CooldownSeconds;
        }

        var spellComponent = pGizmo.GetComponent<SpellComponent>();

        if (spellComponent != null) {
            return spellComponent.CooldownSeconds;
        }

        return 0;
    }
}