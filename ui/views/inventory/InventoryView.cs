using RPG.global;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;

namespace RPG.ui.views.inventory;

public abstract partial class InventoryView : View<GizmoStack> {
    
    public virtual void InitializeWith(Inventory? pContainer) {
        if (Container == pContainer) {
            return;
        }

        if (pContainer is null) {
            return;
        }

        // Disconnect old signals
        if (Container is Inventory inventory) {
            inventory.SizeChanged -= OnInventorySizeChanged;
            inventory.GizmoAboutToChange -= OnInventoryGizmoAboutToChange;
            inventory.GizmoChanged -= OnInventoryGizmoChanged;
        }

        Container = pContainer;

        GetInventory().SizeChanged += OnInventorySizeChanged;
        GetInventory().GizmoAboutToChange += OnInventoryGizmoAboutToChange;
        GetInventory().GizmoChanged += OnInventoryGizmoChanged;

        SetupHolder();
        ResizeHolder();
    }

    private void OnInventoryGizmoAboutToChange(GizmoStack pGizmoStack, int pIndex) {
        var slot = GetSlot<views.inventory.InventorySlot>(pIndex);
        if (pGizmoStack.Gizmo is null || slot is null) {
            return;
        }

        SpellComponent? spellComponent = pGizmoStack.Gizmo?.GetComponent<SpellComponent, ChainSpellComponent>();

        if (spellComponent is null) {
            UpdateSlot(pGizmoStack, slot, 0.0f);
            return;
        }
        
        if (spellComponent.IsCastCompleteConnected(slot.CastCompleteCallback)) {
            spellComponent.DisconnectCastComplete(slot.CastCompleteCallback);
        }
        
        UpdateSlot(pGizmoStack, slot, spellComponent.GetRemainingCooldown());
    }

    private void OnInventoryGizmoChanged(GizmoStack pGizmoStack, int pIndex) {
        var slot = GetSlot<views.inventory.InventorySlot>(pIndex);

        if (slot is null) {
            Logger.Inventory.Critical($"BUG! Slot with an index {pIndex} does not exist.", true);
            return;
        }

        SpellComponent? spellComponent = pGizmoStack.Gizmo?.GetComponent<SpellComponent, ChainSpellComponent>();

        if (spellComponent is null) {
            UpdateSlot(pGizmoStack, slot, 0.0f);
            return;
        }
        
        var error = spellComponent.ConnectCastComplete(slot.CastCompleteCallback);
        UpdateSlot(pGizmoStack, slot, spellComponent.GetRemainingCooldown());
    }

    private void OnInventorySizeChanged() {
        ResizeHolder();
    }

    protected override void AddSlot(int pIndex) {
        if (Container is null) {
            Logger.UI.Critical("BUG! Container should not be null here!", true);
            return;
        }

        GizmoStack gizmoStack = Container.GetAt(pIndex);
        var slot = SlotScene.Instantiate<views.inventory.InventorySlot>();

        
        // FIND_ME:
        slot.Update(gizmoStack);
        // TODO: those signals won't be ever disconnected...
        // https://docs.godotengine.org/en/4.4/tutorials/scripting/c_sharp/c_sharp_signals.html
        slot.LeftMouseButtonPressed += () => EmitSignalSlotPressed(this, slot, false);
        slot.RightMouseButtonPressed += () => EmitSignalSlotPressed(this, slot, true);
        slot.Hovered += () => EmitSignalSlotHovered(this, slot);
        slot.Unhovered += EmitSignalSlotUnhovered;
        SlotHolder.AddChild(slot);
        
        slot.CastCompleteCallback = (float pCooldownSeconds) => {
            UpdateSlot(gizmoStack, slot, pCooldownSeconds);
        };

        SpellComponent? spellComponent = gizmoStack.Gizmo?.GetComponent<SpellComponent, ChainSpellComponent>();

        if (spellComponent is null) {
            // UpdateSlot(gizmoStack, slot, 0.0f);
            return;
        }
        
        spellComponent.ConnectCastComplete(slot.CastCompleteCallback);
    }

    // public static void Rewire(GizmoStack pGizmoStack, InventorySlot pSlot, bool flag) {
    //     Action<float> a = (float pCooldownSeconds) => {
    //         UpdateSlot(pGizmoStack, pSlot, pCooldownSeconds);
    //     };
    //     
    //     Callable castCompleteCallback = Callable.From(a);
    //
    //     if (pGizmoStack.Gizmo is null) {
    //         UpdateSlot(pGizmoStack, pSlot, 0.0f);
    //         return;
    //     }
    //     
    //     SpellComponent? spellComponent = pGizmoStack.Gizmo.GetComponent<SpellComponent, ChainSpellComponent>();
    //
    //     if (spellComponent is null) {
    //         UpdateSlot(pGizmoStack, pSlot, 0.0f);
    //         return;
    //     }
    //
    //     if (flag) {
    //         if (spellComponent.IsCastCompleteConnected(castCompleteCallback)) {
    //             UpdateSlot(pGizmoStack, pSlot, spellComponent.GetRemainingCooldown());
    //             return;
    //         }
    //
    //         Error? error = spellComponent.ConnectCastComplete(castCompleteCallback);
    //         if (error != Error.Ok) {
    //             Logger.UI.Error($"Could not connect signal {SpellComponent.SignalName.CastComplete}. Error code: {error}",
    //                 true);
    //             UpdateSlot(pGizmoStack, pSlot, spellComponent.GetRemainingCooldown());
    //             return;
    //         }
    //         GD.Print($"Wired: {pSlot.GetIndex()}");
    //
    //         UpdateSlot(pGizmoStack, pSlot, spellComponent.GetRemainingCooldown());
    //     } else {
    //         if (!spellComponent.IsCastCompleteConnected(castCompleteCallback)) {
    //             UpdateSlot(pGizmoStack, pSlot, spellComponent.GetRemainingCooldown());
    //             return;
    //         }
    //         GD.Print($"Unwired: {pSlot.GetIndex()}");
    //         spellComponent.DisconnectCastComplete(castCompleteCallback);
    //     }
    // }
    

    // protected static void WireGizmo(GizmoStack pGizmoStack, InventorySlot pSlot) {
    //     Action<float> castCompleteCallback = (float pCooldownSeconds) => {
    //         UpdateSlot(pGizmoStack, pSlot, pCooldownSeconds);
    //     };
    //
    //     if (pGizmoStack.Gizmo is null) {
    //         UpdateSlot(pGizmoStack, pSlot, 0.0f);
    //         return;
    //     }
    //
    //     SpellComponent? spellComponent = pGizmoStack.Gizmo.GetComponent<SpellComponent, ChainSpellComponent>();
    //     if (spellComponent is null) {
    //         UpdateSlot(pGizmoStack, pSlot, 0.0f);
    //         return;
    //     }
    //
    //     if (spellComponent.IsCastCompleteConnected(castCompleteCallback)) {
    //         UpdateSlot(pGizmoStack, pSlot, spellComponent.GetRemainingCooldown());
    //         return;
    //     }
    //
    //     Error? error = spellComponent.ConnectCastComplete(castCompleteCallback);
    //     if (error != Error.Ok) {
    //         Logger.UI.Error($"Could not connect signal {SpellComponent.SignalName.CastComplete}. Error code: {error}",
    //             true);
    //         UpdateSlot(pGizmoStack, pSlot, spellComponent.GetRemainingCooldown());
    //         return;
    //     }
    //     GD.Print($"Wired: {pSlot.GetIndex()}");
    //
    //     UpdateSlot(pGizmoStack, pSlot, spellComponent.GetRemainingCooldown());
    // }
    //
    // protected static void UnwireGizmo(GizmoStack pGizmoStack, InventorySlot pSlot) {
    //     
    //     Action<float> castCompleteCallback = (float pCooldownSeconds) => {
    //         UpdateSlot(pGizmoStack, pSlot, pCooldownSeconds);
    //     };
    //
    //     if (pGizmoStack.Gizmo is null) {
    //         UpdateSlot(pGizmoStack, pSlot, 0.0f);
    //         // GD.Print("a");
    //         return;
    //     }
    //
    //     SpellComponent? spellComponent = pGizmoStack.Gizmo.GetComponent<SpellComponent, ChainSpellComponent>();
    //
    //     if (spellComponent is null) {
    //         UpdateSlot(pGizmoStack, pSlot, 0.0f);
    //         // GD.Print("b");
    //         return;
    //     }
    //
    //     if (!spellComponent.IsCastCompleteConnected(castCompleteCallback)) {
    //         UpdateSlot(pGizmoStack, pSlot, spellComponent.GetRemainingCooldown());
    //         // GD.Print("c");
    //         return;
    //     }
    //     GD.Print($"Unwired: {pSlot.GetIndex()}");
    //     spellComponent.DisconnectCastComplete(castCompleteCallback);
    // }

    // private void RewireGizmo(GizmoStack pGizmoStack, InventorySlot pSlot) {
    //     if (Container is null) {
    //         Logger.UI.Critical("BUG! Container should not be null here!", true);
    //         return;
    //     }
    //
    //     Action<float> castCompleteCallback = (float pCooldownSeconds) => {
    //         UpdateSlot(pGizmoStack, pSlot, pCooldownSeconds);
    //     };
    //
    //     GizmoStack currentGizmoStack = Container.GetAt(pSlot.GetIndex());
    //     Gizmo? currentGizmo = currentGizmoStack.Gizmo;
    //
    //     SpellComponent? currentSpellComponent = currentGizmo?.GetComponent<SpellComponent, ChainSpellComponent>();
    //
    //     // Disconnect old signal...
    //     if (currentSpellComponent != null && currentSpellComponent.IsCastCompleteConnected(castCompleteCallback)) {
    //         currentSpellComponent.DisconnectCastComplete(castCompleteCallback);
    //     }
    //
    //     if (pGizmoStack.Gizmo is null) {
    //         UpdateSlot(pGizmoStack, pSlot, 0.0f);
    //         return;
    //     }
    //
    //     // ...and connect new
    //     SpellComponent? spellComponent = pGizmoStack.Gizmo.GetComponent<SpellComponent, ChainSpellComponent>();
    //     Error? error = spellComponent?.ConnectCastComplete(castCompleteCallback);
    //
    //     if (error is not null && error != Error.Ok) {
    //         Logger.UI.Error($"Could not connect signal {SpellComponent.SignalName.CastComplete}. Error code: {error}",
    //             true);
    //     }
    //
    //     UpdateSlot(pGizmoStack, pSlot, spellComponent?.GetRemainingCooldown() ?? 0.0f);
    // }

    public Inventory GetInventory() {
        return (Inventory)Container!;
    }

    private static void UpdateSlot(GizmoStack pGizmoStack, views.inventory.InventorySlot pSlot, float pCooldownSeconds) {
        pSlot.Update(pGizmoStack);
        pSlot.SetOnCooldown(pCooldownSeconds);
    }
}