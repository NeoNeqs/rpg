using Godot;
using RPG.global;
using RPG.scripts.combat;
using RPG.scripts.effects;
using RPG.scripts.inventory;

namespace RPG.ui;

public partial class EffectView : View<(Gizmo, Effect)> {
    public void InitializeWith(CombatManager? pContainer) {
        if (Container == pContainer) {
            GD.Print("A");
            return;
        }

        if (pContainer is null) {
            GD.Print("B");
            return;
        }

        // Disconnect old signals
        if (Container is CombatManager combatManager) {
            combatManager.AppliedEffect -= OnEffectApplied;
            combatManager.RemovedEffect -= OnEffectRemoved;
            // inventory.SizeChanged -= OnInventorySizeChanged;
            // inventory.GizmoAboutToChange -= OnInventoryGizmoAboutToChange;
            // inventory.GizmoChanged -= OnInventoryGizmoChanged;
        }

        Container = pContainer;
        GetCombatManager().AppliedEffect += OnEffectApplied;
        GetCombatManager().RemovedEffect += OnEffectRemoved;
        // GetInventory().SizeChanged += OnInventorySizeChanged;
        // GetInventory().GizmoAboutToChange += OnInventoryGizmoAboutToChange;
        // GetInventory().GizmoChanged += OnInventoryGizmoChanged;

        SetupHolder();
        ResizeHolder();
    }

    private void OnEffectRemoved(Gizmo pEffectOwner, Effect pEffect, int pIndex) {
        var slot = GetSlot<EffectSlot>(pIndex)!;
        SlotHolder.RemoveChild(slot);
        slot.QueueFree();
    }

    private void OnEffectApplied(Gizmo pEffectOwner, Effect pEffect) {
        ResizeHolder();

        var slot = GetSlot<EffectSlot>(SlotHolder.GetChildCount() - 1)!;
        slot.Update((pEffectOwner, pEffect));
        
    }

    protected override void AddSlot(int pIndex) {
        if (Container is null) {
            Logger.UI.Critical("BUG! Container should not be null here!", true);
            return;
        }

        (Gizmo, Effect) data = Container.GetAt(pIndex);
        var slot = SlotScene.Instantiate<EffectSlot>();
        slot.Update(data);
        
        slot.LeftMouseButtonPressed += () => EmitSignalSlotPressed(this, slot, false);
        slot.RightMouseButtonPressed += () => EmitSignalSlotPressed(this, slot, true);
        slot.Hovered += () => EmitSignalSlotHovered(this, slot);
        slot.Unhovered += EmitSignalSlotUnhovered;
        SlotHolder.AddChild(slot);
        
        
        
    }
    
    public CombatManager GetCombatManager() {
        return (CombatManager)Container!;
    }

}