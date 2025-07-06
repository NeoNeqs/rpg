using System.Collections.Generic;
using Godot;
using RPG.scripts.combat;
using RPG.scripts.effects;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;
using RPG.ui.item;

namespace RPG.ui;

public partial class EntityFrame : UIElement {
    private GridContainer Grid => GetNodeOrNull<GridContainer>("VBoxContainer/GridContainer");
    private static PackedScene ItemSlotScene => GD.Load<PackedScene>("uid://b7xu5k302lfn1");

    private CombatManager? _combatManager;

    public void Update(CombatManager? pCombatManager) {
        if (_combatManager == pCombatManager) {
            return;
        }

        foreach (Node? node in Grid.GetChildren()) {
            Grid.RemoveChild(node);
            node.QueueFree();
        }

        if (_combatManager is not null) {
            _combatManager.AppliedEffect -= OnEffectApplied;
        }

        if (pCombatManager is not null) {
            foreach ((Gizmo pEffectOwner, Effect pEffect) in pCombatManager.GetAppliedEffects().Values) {
                OnEffectApplied(pEffectOwner, pEffect);
            }

            pCombatManager.AppliedEffect += OnEffectApplied;
        }

        _combatManager = pCombatManager;
        // pCombatManager.RemovedEffect += (Gizmo pOwner, Effect pEffect) => { };
    }

    private void OnEffectApplied(Gizmo pEffectOwner, Effect pEffect) {
        var itemSlot = ItemSlotScene.Instantiate<ItemSlot>();
        itemSlot.CustomMinimumSize = new Vector2(20, 20);

        var gs = new GizmoStack();
        gs.Gizmo = pEffectOwner;
        itemSlot.Update(gs);
        
        // pEffect.Finished += asd;
        pEffect.Finished += () => {
            if (IsInstanceValid(itemSlot)) {
                Grid.RemoveChild(itemSlot);
                itemSlot.QueueFree();
            }
        };
        
        SpellComponent? sp = pEffectOwner.GetComponent<SpellComponent, ChainSpellComponent>();
        if (sp is null) {
            return;
        }

        sp.CastComplete += _ => {
            if (IsInstanceValid(itemSlot)) {
                itemSlot.SetOnCooldown(pEffect.GetTimeLeft());
            }
        };
        Grid.AddChild(itemSlot);

        if (IsInstanceValid(itemSlot)) {
            itemSlot.SetOnCooldown(pEffect.GetTimeLeft());
        }
    }

    public void asd(ItemSlot slot) {
        
    }
}