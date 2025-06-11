using System;
using System.Threading.Tasks;
using Godot;
using RPG.global;
using RPG.scripts.inventory;

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
        Inventory.GizmoAboutToChange += OnInventoryGizmoUpdate;
        Inventory.GizmoChanged += OnInventoryGizmoUpdate;
   
        SetupContainer();
        ResizeContainer();
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

        UpdateSlot(pGizmoStack, slot, pGizmoStack.Gizmo?.GetCooldown() ?? 0);
    }

    protected virtual void ResizeContainer() {
        int oldSize = Container.GetChildCount();
        int newSize = Inventory.GetSize();

        int slotsToAdd = Math.Max(0, newSize - oldSize);
        int slotsToRemove = Math.Max(0, oldSize - newSize);

        for (int i = 0; i < slotsToAdd; i++) {
            AddSlot(Inventory.At(oldSize + i));
        }

        for (int i = 0; i < slotsToRemove; i++) {
            Node? slot = Container.GetChildOrNull<Node?>(oldSize - 1 - i);
            slot?.QueueFree();
        }

        // Shrink container to tightly fit all the slots
        Size = CustomMinimumSize;
    }

    private void AddSlot(GizmoStack pGizmoStack) {
        InventorySlot? slot = SlotScene.Instantiate<InventorySlot>();
        slot.Update(pGizmoStack);
        slot.LeftMouseButtonPressed += () => EmitSignalSlotPressed(this, slot, false);
        slot.RightMouseButtonPressed += () => EmitSignalSlotPressed(this, slot, true);
        slot.Hovered += () => EmitSignalSlotHovered(this, slot);
        slot.Unhovered += EmitSignalSlotUnhovered;
        Container.AddChild(slot);

        RewireGizmo(pGizmoStack, slot);
    }

    private void RewireGizmo(GizmoStack pGizmoStack, InventorySlot pSlot) {
        pSlot.ResetCooldown();

        if (pGizmoStack.Gizmo is null) {
            UpdateSlot(pGizmoStack, pSlot, 0);
            return;
        }

        Callable onGizmoCastedCallable = Callable.From((ulong pCooldownInMicroSeconds) => {
            UpdateSlot(pGizmoStack, pSlot, pCooldownInMicroSeconds);
        });


        // Disconnect old signal...
        GizmoStack currentGizmoStack = Inventory.At(pSlot.GetIndex());
        if (currentGizmoStack != pGizmoStack && currentGizmoStack.Gizmo is not null) {
            // I hate working with Godot signals in C#
            // Waiting for this to be even considered by Godot's Team:
            // https://github.com/godotengine/godot-proposals/issues/12269
            // My guess is 2030 
            if (currentGizmoStack.Gizmo.IsConnected(Gizmo.SignalName.Casted, onGizmoCastedCallable)) {
                currentGizmoStack.Gizmo.Disconnect(Gizmo.SignalName.Casted, onGizmoCastedCallable);
            }
        }
        
        // ...and connect new
        pGizmoStack.Gizmo.Connect(Gizmo.SignalName.Casted, onGizmoCastedCallable);

        UpdateSlot(pGizmoStack, pSlot, pGizmoStack.Gizmo.GetRemainingCooldown());
    }

    private static void UpdateSlot(GizmoStack pGizmoStack, InventorySlot pSlot, ulong pCooldownInMicroSeconds) {
        pSlot.Update(pGizmoStack);
        pSlot.SetOnCooldown(pCooldownInMicroSeconds);
    }
}