using System;
using System.Threading.Tasks;
using Godot;
using RPG.Global;
using RPG.scripts.inventory;

namespace RPG.ui.inventory;

public abstract partial class InventoryView : UIElement {
    [Signal]
    public delegate void SlotPressedEventHandler(InventoryView pSourceView, InventorySlot pSlot, bool pIsRightClick);

    [Signal]
    public delegate void SlotHoveredEventHandler(InventorySlot pSlot);

    [Signal]
    public delegate void SlotUnhoveredEventHandler();


    [Export] public Container Container;
    [Export] public PackedScene SlotScene;
    [Export] public Inventory Inventory;

    public async Task Initialize(Inventory pInventory) {
        Inventory = pInventory;
        Inventory.SizeChanged += OnInventorySizeChanged;
        Inventory.GizmoAboutToChange += OnInventoryGizmoUpdate;
        Inventory.GizmoChanged += OnInventoryGizmoUpdate;

        if (!IsNodeReady()) {
            await ToSignal(this, Node.SignalName.Ready);
        }

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

        slot.Update(pGizmoStack);
    }
    // private void OnInventoryGizmoAboutToChange(GizmoStack pGizmoStack, int pIndex) {
    //     InventorySlot? slot = GetSlot(pIndex);
    //
    //     if (slot is null) {
    //         Logger.Inventory.Error($"BUG! Slot with an index {pIndex} does not exist.", true);
    //         return;
    //     }
    //     
    //     slot.Update(pGizmoStack);
    // }
    //
    // private void OnInventoryGizmoChanged(GizmoStack pGizmoStack, int pIndex) {
    //     InventorySlot? slot = GetSlot(pIndex);
    //
    //     if (slot is null) {
    //         Logger.Inventory.Error($"BUG! Slot with an index {pIndex} does not exist.", true);
    //         return;
    //     }
    //     
    //     slot.Update(pGizmoStack);
    // }

    private void ResizeContainer() {
        int oldSize = Container.GetChildCount();
        int newSize = Inventory.GetSize();

        int slotsToAdd = Math.Max(0, newSize - oldSize);
        int slotsToRemove = Math.Max(0, oldSize - newSize);

        for (int i = 0; i < slotsToAdd; i++) {
            InventorySlot slot = CreateSlot(Inventory.At(oldSize + i));
            Container.AddChild(slot);
        }

        for (int i = 0; i < slotsToRemove; i++) {
            Node? slot = Container.GetChildOrNull<Node?>(oldSize - 1 - i);
            slot?.QueueFree();
        }

        // Shrink container to tightly fit all the slots
        Size = CustomMinimumSize;
    }

    private InventorySlot CreateSlot(GizmoStack pGizmoStack) {
        InventorySlot? slot = SlotScene.Instantiate<InventorySlot>();
        slot.Update(pGizmoStack);
        slot.LeftMouseButtonPressed += () => EmitSignalSlotPressed(this, slot, false);
        slot.RightMouseButtonPressed += () => EmitSignalSlotPressed(this, slot, true);
        slot.Hovered += () => EmitSignalSlotHovered(slot);
        slot.Unhovered += EmitSignalSlotUnhovered;

        SetupGizmo(pGizmoStack, slot);
        return slot;
    }

    private static void SetupGizmo(GizmoStack pGizmoStack, InventorySlot pSlot) {
        pSlot.ResetCooldown();

        if (pGizmoStack.Gizmo is null) {
            return;
        }

        // For some reason, this has to be explicitly cast to Action<ulong> ???
        // The advantage of this is that at most, 1 delegate allocation will happen instead of 3 in the worst case. 
        Callable onGizmoCastedCallable = Callable.From((Action<ulong>)OnGizmoCasted);

        if (pGizmoStack.Gizmo.IsConnected(Gizmo.SignalName.Casted, onGizmoCastedCallable)) {
            pGizmoStack.Gizmo.Disconnect(Gizmo.SignalName.Casted, onGizmoCastedCallable);
        }

        pGizmoStack.Gizmo.Connect(Gizmo.SignalName.Casted, onGizmoCastedCallable);

        // A Gizmo might be already on cooldown so update it!
        ulong remainingCooldown = pGizmoStack.Gizmo.GetRemainingCooldown();
        if (remainingCooldown > 0) {
            UpdateSlot(pGizmoStack, pSlot, remainingCooldown);
        }

        return; // no, this return is not misplaced...

        // I hate working with Godot signals in C#
        // Waiting for this to be even considered by Godot's Team:
        // https://github.com/godotengine/godot-proposals/issues/12269
        // My guess is 5 more years
        void OnGizmoCasted(ulong pCooldownInMicroSeconds) {
            UpdateSlot(pGizmoStack, pSlot, pCooldownInMicroSeconds);
        }
    }

    private static void UpdateSlot(GizmoStack pGizmoStack, InventorySlot pSlot, ulong pCooldownInMicroSeconds) {
        pSlot.Update(pGizmoStack);
        pSlot.SetOnCooldown(pCooldownInMicroSeconds);
    }
}