using System;
using Godot;
using RPG.global;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;

namespace RPG.ui.inventory;

public abstract partial class InventoryView : View {
    [Signal]
    public delegate void SlotPressedEventHandler(InventoryView pSourceView, InventorySlot pSlot, bool pIsRightClick);

    [Signal]
    public delegate void SlotHoveredEventHandler(InventoryView pSourceView, InventorySlot pSlot);

    [Signal]
    public delegate void SlotUnhoveredEventHandler();


    [Export] public Container Container = null!;
    [Export] public PackedScene SlotScene = null!;
    public Inventory Inventory = null!;

    public virtual void SetData(Inventory pInventory) {
        Inventory = pInventory;
        Inventory.SizeChanged += OnInventorySizeChanged;
        Inventory.GizmoAboutToChange += OnInventoryGizmoAboutToChange;
        Inventory.GizmoChanged += OnInventoryGizmoUpdate;
        

        SetupContainer();
        ResizeContainer();
    }

    private void OnInventoryGizmoAboutToChange(GizmoStack pGizmoStack, int pIndex) {
        InventorySlot? slot = GetSlot(pIndex);
        if (pGizmoStack.Gizmo is null || slot is null) {
            return;
        }

        RewireGizmo(pGizmoStack, slot);

        // SpellComponent? spellComponent = pGizmoStack.Gizmo.GetComponent<SpellComponent, ChainSpellComponent>();
        // if (spellComponent is not null) {
        //     spellComponent.CastComplete -= slot.SetOnCooldown;
        // }

        slot.ResetCooldown();
    }

    public InventorySlot? GetSlot(int pIndex) {
        return Container.GetChildOrNull<InventorySlot>(pIndex);
    }

    protected abstract void SetupContainer();

    private void OnInventorySizeChanged() {
        ResizeContainer();
    }

    private void OnInventoryGizmoUpdate(GizmoStack pGizmoStack, int pIndex) {
        InventorySlot? slot = GetSlot(pIndex);

        if (slot is null) {
            Logger.Inventory.Error($"BUG! Slot with an index {pIndex} does not exist.", true);
            return;
        }

        RewireGizmo(pGizmoStack, slot);
    }

    protected virtual void ResizeContainer() {
        int oldSize = Container.GetChildCount();
        int newSize = Inventory.GetSize();

        int slotsToAdd = Math.Max(0, newSize - oldSize);
        int slotsToRemove = Math.Max(0, oldSize - newSize);

        for (int i = 0; i < slotsToAdd; i++) {
            AddSlot(Inventory.GetAt(oldSize + i));
        }

        for (int i = 0; i < slotsToRemove; i++) {
            var slot = Container.GetChildOrNull<Node?>(oldSize - 1 - i);
            slot?.QueueFree();
        }

        // Shrink container to tightly fit all the slots
        Size = CustomMinimumSize;
    }

    private void AddSlot(GizmoStack pGizmoStack) {
        var slot = SlotScene.Instantiate<InventorySlot>();
        slot.Update(pGizmoStack);
        slot.LeftMouseButtonPressed += () => EmitSignalSlotPressed(this, slot, false);
        slot.RightMouseButtonPressed += () => EmitSignalSlotPressed(this, slot, true);
        slot.Hovered += () => EmitSignalSlotHovered(this, slot);
        slot.Unhovered += EmitSignalSlotUnhovered;
        Container.AddChild(slot);

        RewireGizmo(pGizmoStack, slot);
    }

    private void RewireGizmo(GizmoStack pGizmoStack, InventorySlot pSlot) {
        Action<float> castCompleteCallback = (float pCooldownSeconds) => {
            UpdateSlot(pGizmoStack, pSlot, pCooldownSeconds);
        };
        
        // Disconnect old signal...
        GizmoStack currentGizmoStack = Inventory.GetAt(pSlot.GetIndex());
        
        SpellComponent? currentSpellComponent = currentGizmoStack.Gizmo?.GetComponent<SpellComponent, ChainSpellComponent>();
        if (currentSpellComponent != null && currentSpellComponent.IsCastCompleteConnected(castCompleteCallback)) {
            currentSpellComponent.DisconnectCastComplete(castCompleteCallback);
        }

        if (pGizmoStack.Gizmo is null) {
            UpdateSlot(pGizmoStack, pSlot, 0.0f);
            return;
        }

        // ...and connect new
        SpellComponent? spellComponent = pGizmoStack.Gizmo.GetComponent<SpellComponent, ChainSpellComponent>();
        Error? error = spellComponent?.ConnectCastComplete(castCompleteCallback);
        
        if (error is not null && error != Error.Ok) {
            Logger.UI.Error($"Could not connect signal {SpellComponent.SignalName.CastComplete}. Error code: {error}", true);
        }

        UpdateSlot(pGizmoStack, pSlot, spellComponent?.GetRemainingCooldown() ?? 0.0f);
    }

    private static void UpdateSlot(GizmoStack pGizmoStack, InventorySlot pSlot, float pCooldownSeconds) {
        pSlot.Update(pGizmoStack);
        pSlot.SetOnCooldown(pCooldownSeconds);
    }
}