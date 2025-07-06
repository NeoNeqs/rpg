using System;
using Godot;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;

namespace RPG.ui.inventory;

[GlobalClass]
public abstract partial class InventorySlot : Slot {
    
    public Action<float> CastCompleteCallback = null!;
    
    public virtual void Update(GizmoStack? pGizmoStack) {
    }
   
}