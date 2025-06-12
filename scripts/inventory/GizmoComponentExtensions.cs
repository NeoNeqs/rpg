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

    public static (Array<Effect>, float) GetAoeInfo(this Gizmo pGizmo) {
        SpellComponent? spellComponent = pGizmo.GetComponent<SpellComponent, ChainSpellComponent>();

        if (spellComponent is null) {
            return ([], 0.0f);
        }

        Array<Effect> aoeEffects = [];
        float maxEffectRange = float.NegativeInfinity;
        // AreaOfEffectComponent? firstAreaEffect = null;
        // Figure out if any of effects are AoE
        foreach (Effect effect in spellComponent.Effects) {
            var aoe = effect.GetComponent<AreaOfEffectComponent>();
            
            if (aoe is not null) {
                aoeEffects.Add(effect);
                if (effect.MaxRange > maxEffectRange) {
                    maxEffectRange = effect.MaxRange;
                }
            }

            break;
        }

        return (aoeEffects, maxEffectRange);
    }
}