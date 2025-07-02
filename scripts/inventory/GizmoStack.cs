using System;
using Godot;
using Godot.Collections;
using RPG.scripts.inventory.components;

namespace RPG.scripts.inventory;

[Tool, GlobalClass]
public partial class GizmoStack : Resource {
    [Export]
    public Gizmo? Gizmo {
        set {
            if (_quantity == 0) {
                _quantity = 1;
            }
            _gizmo = value;
        }
        get => _gizmo;
    }

    [Export]
    public long Quantity {
        set {
            if (value == 0) {
                _gizmo = null;
            }
            _quantity = value;
        }
        get => _gizmo is null ? 0 : Math.Clamp(_quantity, 0, _gizmo.StackSize);
    }
    
    [Export] public Array<GizmoComponent> AllowedComponents = [];

    private Gizmo? _gizmo;
    private long _quantity;
    
    public bool IsEmpty() {
        return _quantity == 0;
    }
}