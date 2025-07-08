using System;
using Godot;
using RPG.scripts.inventory;

namespace RPG.ui.views.inventory;

[GlobalClass]
public abstract partial class InventorySlot : Slot {
    
    public Action<float> CastCompleteCallback = null!;
    
    [Export] public Label TextHolder = null!;
    
    public virtual void Update(GizmoStack? pGizmoStack) {
    }
   
}