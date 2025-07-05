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
    
    private CombatManager? _combatManager = null;

    public void Update(CombatManager pCombatManager) {
        if (_combatManager == pCombatManager) {
            return;
        }
        // save pCombatManger to a field and remember to disconnect all signal / clear old elements


        foreach (Node? node in Grid.GetChildren()) {
            Grid.RemoveChild(node);
            node.QueueFree();
        }

        if (_combatManager is not null) {
            _combatManager.AppliedEffect -= OnEffectApplied;
        }

        pCombatManager.AppliedEffect += OnEffectApplied;

        _combatManager = pCombatManager;
        // pCombatManager.RemovedEffect += (Gizmo pOwner, Effect pEffect) => { };
    }

    private void OnEffectApplied(Gizmo pEffectOwner, Effect pEffect) {
        var itemSlot = ItemSlotScene.Instantiate<ItemSlot>();
        itemSlot.CustomMinimumSize = new Vector2(20, 20);
        Grid.AddChild(itemSlot);
        var gs = new GizmoStack();
        gs.Gizmo = pEffectOwner;
        itemSlot.Update(gs);
        
        pEffect.Finished += itemSlot.QueueFree;
        var sp = pEffectOwner.GetComponent<SpellComponent, ChainSpellComponent>();
        if (sp is null) {
            return;
        }

        // sp.CastComplete += pCooldownInMicroSeconds => {
        //     itemSlot.SetOnCooldown(pCooldownInMicroSeconds);
        // };
    }
}