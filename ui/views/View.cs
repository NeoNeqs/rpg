using System;
using global::RPG.global;
using Godot;
using RPG.ui.elements;

namespace RPG.ui.views;

public abstract partial class View<TItem> : UIElement {
    [Signal]
    public delegate void SlotHoveredEventHandler(View<TItem> pSourceView, Slot pSlot);

    [Signal]
    public delegate void SlotPressedEventHandler(View<TItem> pSourceView, Slot pSlot, bool pIsRightClick);

    [Signal]
    public delegate void SlotUnhoveredEventHandler();

    protected IContainer<TItem>? Container;

    [Export] public Container SlotHolder = null!;
    [Export] public PackedScene SlotScene = null!;

    public TSlot? GetSlot<TSlot>(int pIndex) where TSlot : Slot {
        return SlotHolder.GetChildOrNull<TSlot>(pIndex);
    }

    protected virtual void SetupHolder() { }

    protected abstract void AddSlot(int pIndex);

    protected void ResizeHolder() {
        if (Container is null) {
            return;
        }

        int oldSize = SlotHolder.GetChildCount();
        int newSize = Container.GetSize();

        int slotsToAdd = Math.Max(0, newSize - oldSize);
        int slotsToRemove = Math.Max(0, oldSize - newSize);

        for (int i = 0; i < slotsToAdd; i++) {
            AddSlot(oldSize + i);
        }

        for (int i = 0; i < slotsToRemove; i++) {
            var slot = SlotHolder.GetChildOrNull<Node?>(oldSize - 1 - i);
            slot?.QueueFree();
        }

        // Shrink container to tightly fit all the slots
        Size = CustomMinimumSize;
    }
}