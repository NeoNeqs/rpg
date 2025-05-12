using System;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Collections;
using RPG.Global;
using RPG.scripts.components;

namespace RPG.scripts.inventory;

/// <summary>
///     <para>Holds <see cref="GizmoStack"/>s. This class has 2 guarantees in place:</para>
///     <para>1. Every <see cref="GizmoStack"/> is initialized and never null</para>
///     <para>2. <see cref="GizmoStack"/> never changes the position.</para>
/// </summary>
public partial class Inventory : Resource {
    [Signal]
    public delegate void GizmoAboutToChangeEventHandler(GizmoStack pGizmo);

    [Signal]
    public delegate void GizmoChangedEventHandler(GizmoStack pGizmo);

    [Signal]
    public delegate void SizeChangedEventHandler();

    public enum ActionResult : byte {
        None,
        Leftover,
    }

    [Flags]
    public enum InventoryFlags : byte {
        Owning = 1 << 0,
        Editable = 1 << 1,
    }

    [Export]
    public Array<GizmoStack> Gizmos {
        set => SetGizmos(value);
        get => _gizmos;
    }

    private Array<GizmoStack> _gizmos = [];

    [Export] public int Columns;
    [Export] public required Array<GizmoComponent> AllowedComponents;
    [Export] public InventoryFlags Flags;

    public ActionResult HandleGizmoAction(int pFrom, Inventory pToInventory, int pTo, bool pSingle) {
        GizmoStack fromGizmoStack = At(pFrom);
        GizmoStack toGizmoStack = pToInventory.At(pTo);

        if (!IsIndexInRange(pFrom) || !pToInventory.IsIndexInRange(pTo)) {
            return ActionResult.None;
        }

        if (fromGizmoStack.IsEmpty()) {
            return ActionResult.None;
        }

        if (this == pToInventory && pFrom == pTo) {
            return ActionResult.None;
        }

        if (!IsAllowed(fromGizmoStack, pToInventory, toGizmoStack)) {
            return ActionResult.None;
        }

        if (!IsEditable()) {
            if (pToInventory.IsEditable()) {
                return Reference(fromGizmoStack, pToInventory, toGizmoStack);
            }

            return ActionResult.None;
        }

        if (IsOwning() && !pToInventory.IsOwning()) {
            return Reference(fromGizmoStack, pToInventory, toGizmoStack);
        }

        if (!IsOwning() && pToInventory.IsOwning()) {
            return ActionResult.None;
        }

        if (toGizmoStack.IsEmpty()) {
            return Move(fromGizmoStack, pToInventory, toGizmoStack, pSingle);
        }

        if (!IsAllowed(toGizmoStack, this, fromGizmoStack)) {
            return ActionResult.None;
        }

        if (toGizmoStack.Quantity == toGizmoStack.Gizmo?.StackSize) {
            return Swap(fromGizmoStack, pToInventory, toGizmoStack);
        }

        if (fromGizmoStack.Gizmo == toGizmoStack.Gizmo && IsOwning() && !pToInventory.IsOwning()) {
            return Stack(fromGizmoStack, pToInventory, toGizmoStack, pSingle);
        }

        return Swap(fromGizmoStack, pToInventory, toGizmoStack);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GizmoStack At(int pIndex) {
        return _gizmos[pIndex];
    }

    private static bool IsAllowed(GizmoStack pFromGizmoStack, Inventory pToInventory, GizmoStack pToGizmoStack) {
        foreach (GizmoComponent allowedComponent in pToGizmoStack.AllowedComponents) {
            GizmoComponent? checkComponent = pFromGizmoStack.Gizmo?.GetComponent(allowedComponent);

            if (checkComponent is null) {
                continue;
            }

            if (allowedComponent.IsAllowed(checkComponent)) {
                return true;
            }
        }

        if (pToGizmoStack.AllowedComponents.Count != 0) {
            return false;
        }

        foreach (GizmoComponent inventoryAllowedComponent in pToInventory.AllowedComponents) {
            GizmoComponent? checkComponent = pFromGizmoStack.Gizmo?.GetComponent(inventoryAllowedComponent);

            if (checkComponent is null) {
                continue;
            }

            if (inventoryAllowedComponent.IsAllowed(checkComponent)) {
                return true;
            }
        }

        return false;
    }

    private ActionResult Move(GizmoStack pFromGizmoStack, Inventory pToInv, GizmoStack pToGizmoStack, bool pSingle) {
        if (pSingle && IsOwning()) {
            EmitSignalGizmoAboutToChange(pFromGizmoStack);
            pToInv.EmitSignalGizmoAboutToChange(pToGizmoStack);

            pToGizmoStack.Gizmo = pFromGizmoStack.Gizmo;

            pFromGizmoStack.Quantity -= 1;

            if (pToGizmoStack.Quantity > 1) {
                pToGizmoStack.Quantity += 1;
            }

            EmitSignalGizmoChanged(pFromGizmoStack);
            pToInv.EmitSignalGizmoChanged(pToGizmoStack);
        } else {
            Swap(pFromGizmoStack, pToInv, pToGizmoStack);
        }

        if (pToGizmoStack.IsEmpty()) {
            return ActionResult.None;
        }

        return ActionResult.Leftover;
    }

    private ActionResult Swap(GizmoStack pFromGizmoStack, Inventory pToInventory, GizmoStack pToGizmoStack) {
        Gizmo? tempGizmo = pToGizmoStack.Gizmo;
        long tempQuantity = pToGizmoStack.Quantity;

        EmitSignalGizmoAboutToChange(pFromGizmoStack);
        pToInventory.EmitSignalGizmoAboutToChange(pToGizmoStack);

        pToGizmoStack.Gizmo = pFromGizmoStack.Gizmo;
        pToGizmoStack.Quantity = pFromGizmoStack.Quantity;

        pFromGizmoStack.Gizmo = tempGizmo;
        pFromGizmoStack.Quantity = tempQuantity;

        EmitSignalGizmoChanged(pFromGizmoStack);
        pToInventory.EmitSignalGizmoChanged(pToGizmoStack);

        return ActionResult.Leftover;
    }

    private ActionResult Stack(GizmoStack pFromGizmoStack, Inventory pToInv, GizmoStack pToGizmoStack, bool pSingle) {
        long total = pFromGizmoStack.Quantity + pToGizmoStack.Quantity;
        long takeFromTotal;

        if (pSingle) {
            takeFromTotal = Math.Min(pToGizmoStack.Quantity + 1, pToGizmoStack.Gizmo?.StackSize ?? 0);
        } else {
            takeFromTotal = Math.Min(total, pToGizmoStack.Gizmo?.StackSize ?? 0);
        }

        long quantityLeft = Math.Min(0, total - takeFromTotal);

        pToGizmoStack.Quantity = takeFromTotal;
        pFromGizmoStack.Quantity = quantityLeft;

        EmitSignalGizmoChanged(pFromGizmoStack);
        pToInv.EmitSignalGizmoChanged(pToGizmoStack);

        if (pFromGizmoStack.IsEmpty()) {
            return ActionResult.None;
        }

        return ActionResult.Leftover;
    }

    private static ActionResult Reference(GizmoStack pFromGizmoStack, Inventory pToInv, GizmoStack pToGizmoStack) {
        pToInv.EmitSignalGizmoAboutToChange(pToGizmoStack);

        pToGizmoStack.Gizmo = pFromGizmoStack.Gizmo;
        pToGizmoStack.Quantity = pFromGizmoStack.Quantity;

        pToInv.EmitSignalGizmoChanged(pToGizmoStack);

        return ActionResult.None;
    }

    private bool IsEditable() {
        return (Flags & InventoryFlags.Editable) != 0;
    }

    private bool IsOwning() {
        return (Flags & InventoryFlags.Owning) != 0;
    }

    private bool IsIndexInRange(int pIndex) {
        if (pIndex >= 0 && pIndex < _gizmos.Count) {
            return true;
        }

        Logger.Core.Error($"Index out of range of inventory array. Index: {pIndex}, size: {_gizmos.Count}");
        return false;
    }

    private void SetGizmos(Array<GizmoStack> pNewGizmos) {
        if (_gizmos.Count != pNewGizmos.Count) {
            EmitSignalSizeChanged();
        }

        _gizmos = pNewGizmos;
        for (var i = 0; i < _gizmos.Count; i++) {
            if ((GizmoStack?)_gizmos[i] is null) {
                _gizmos[i] = new GizmoStack();
            }
        }
    }
}