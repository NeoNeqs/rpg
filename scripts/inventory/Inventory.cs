using System;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Collections;
using RPG.global;
using GizmoComponent = RPG.scripts.inventory.components.GizmoComponent;

namespace RPG.scripts.inventory;

/// <summary>
///     <para>Holds <see cref="GizmoStack"/>s. This class has 2 guarantees in place:</para>
///     <para>1. Every <see cref="GizmoStack"/> is initialized and never null</para>
///     <para>2. <see cref="GizmoStack"/> never changes the position.</para>
/// </summary>
[Tool, GlobalClass]
public partial class Inventory : Resource {
    [Signal]
    public delegate void GizmoAboutToChangeEventHandler(GizmoStack pGizmo, int pIndex);

    [Signal]
    public delegate void GizmoChangedEventHandler(GizmoStack pGizmo, int pIndex);

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

    // TODO: Not every view makes use of Columns property. Might be a good idea to make into a component, 
    //       when more of those properties come up.
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
                return Reference(pFrom, pToInventory, pTo);
            }

            return ActionResult.None;
        }

        if (IsOwning() && !pToInventory.IsOwning()) {
            return Reference(pFrom, pToInventory, pTo);
        }

        if (!IsOwning() && pToInventory.IsOwning()) {
            return ActionResult.None;
        }

        if (toGizmoStack.IsEmpty()) {
            return Move(pFrom, pToInventory, pTo, pSingle);
            // return Move(fromGizmoStack, pToInventory, toGizmoStack, pSingle);
        }

        if (!IsAllowed(toGizmoStack, this, fromGizmoStack)) {
            return ActionResult.None;
        }

        if (toGizmoStack.Quantity == toGizmoStack.Gizmo?.StackSize) {
            return Swap(pFrom, pToInventory, pTo);
        }

        if (fromGizmoStack.Gizmo == toGizmoStack.Gizmo && IsOwning() && !pToInventory.IsOwning()) {
            return Stack(pFrom, pToInventory, pTo, pSingle);
        }

        return Swap(pFrom, pToInventory, pTo);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GizmoStack At(int pIndex) {
        return _gizmos[pIndex];
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetSize() {
        return Gizmos.Count;
    }

    public bool Delete(int pFrom) {
        if (!IsEditable()) {
            return false;
        }
        
        EmitSignalGizmoAboutToChange(_gizmos[pFrom], pFrom);
        _gizmos[pFrom].Gizmo = null;
        EmitSignalGizmoChanged(_gizmos[pFrom], pFrom);

        return true;
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

    private ActionResult Move(int pFrom, Inventory pToInventory, int pTo, bool pSingle) {
        GizmoStack fromGizmoStack = At(pFrom);
        GizmoStack toGizmoStack = pToInventory.At(pTo);
        
        if (pSingle && IsOwning()) {
            EmitSignalGizmoAboutToChange(fromGizmoStack, pFrom);
            pToInventory.EmitSignalGizmoAboutToChange(toGizmoStack, pTo);

            toGizmoStack.Gizmo = fromGizmoStack.Gizmo;

            fromGizmoStack.Quantity -= 1;

            if (toGizmoStack.Quantity > 1) {
                toGizmoStack.Quantity += 1;
            }

            EmitSignalGizmoChanged(fromGizmoStack, pFrom);
            pToInventory.EmitSignalGizmoChanged(toGizmoStack, pTo);
        } else {
            Swap(pFrom, pToInventory, pTo);
        }

        if (toGizmoStack.IsEmpty()) {
            return ActionResult.None;
        }

        return ActionResult.Leftover;
    }

    private ActionResult Swap(int pFrom, Inventory pToInventory, int pTo) {
        GizmoStack fromGizmoStack = At(pFrom);
        GizmoStack toGizmoStack = pToInventory.At(pTo);
        
        Gizmo? tempGizmo = toGizmoStack.Gizmo;
        long tempQuantity = toGizmoStack.Quantity;

        EmitSignalGizmoAboutToChange(fromGizmoStack, pFrom);
        pToInventory.EmitSignalGizmoAboutToChange(toGizmoStack, pTo);

        toGizmoStack.Gizmo = fromGizmoStack.Gizmo;
        toGizmoStack.Quantity = fromGizmoStack.Quantity;

        fromGizmoStack.Gizmo = tempGizmo;
        fromGizmoStack.Quantity = tempQuantity;

        EmitSignalGizmoChanged(fromGizmoStack, pFrom);
        pToInventory.EmitSignalGizmoChanged(toGizmoStack, pTo);

        return ActionResult.Leftover;
    }

    private ActionResult Stack(int pFrom, Inventory pToInventory, int pTo, bool pSingle) {
        GizmoStack fromGizmoStack = At(pFrom);
        GizmoStack toGizmoStack = pToInventory.At(pTo);
        
        long total = fromGizmoStack.Quantity + toGizmoStack.Quantity;
        long takeFromTotal;

        if (pSingle) {
            takeFromTotal = Math.Min(toGizmoStack.Quantity + 1, toGizmoStack.Gizmo?.StackSize ?? 0);
        } else {
            takeFromTotal = Math.Min(total, toGizmoStack.Gizmo?.StackSize ?? 0);
        }

        long quantityLeft = Math.Min(0, total - takeFromTotal);

        EmitSignalGizmoAboutToChange(fromGizmoStack, pFrom);
        pToInventory.EmitSignalGizmoAboutToChange(toGizmoStack, pTo);
        
        toGizmoStack.Quantity = takeFromTotal;
        fromGizmoStack.Quantity = quantityLeft;

        EmitSignalGizmoChanged(fromGizmoStack, pFrom);
        pToInventory.EmitSignalGizmoChanged(toGizmoStack, pTo);

        if (fromGizmoStack.IsEmpty()) {
            return ActionResult.None;
        }

        return ActionResult.Leftover;
    }

    private ActionResult Reference(int pFrom, Inventory pToInventory, int pTo) {
        GizmoStack fromGizmoStack = At(pFrom);
        GizmoStack toGizmoStack = pToInventory.At(pTo);
        
        pToInventory.EmitSignalGizmoAboutToChange(toGizmoStack, pTo);

        toGizmoStack.Gizmo = fromGizmoStack.Gizmo;
        toGizmoStack.Quantity = fromGizmoStack.Quantity;

        pToInventory.EmitSignalGizmoChanged(toGizmoStack, pTo);

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
        for (int i = 0; i < _gizmos.Count; i++) {
            if ((GizmoStack?)_gizmos[i] is null) {
                _gizmos[i] = new GizmoStack();
            }
        }
    }
}