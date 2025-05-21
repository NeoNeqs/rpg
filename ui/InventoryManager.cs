using global::RPG.global;
using Godot;

namespace RPG.ui;

public partial class InventoryManager : Control {
    // [Export] private Tooltip Tooltip;
    // [Export] private InventorySlot DragSlot;
    //
    // private long _selectedIndex = -1;
    // private InventoryView? _selectedInventoryView = null;

    public override void _EnterTree() {
        EventBus.Instance.CharacterInventoryLoaded += CreateItemInventory;
    }
}