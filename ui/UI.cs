using RPG.global;
using Godot;

namespace RPG.ui;

public partial class UI : Control {

    // [Export]
    // public InventoryManager InventoryManager;

    public override void _GuiInput(InputEvent pEvent) {
        if (pEvent is InputEventMouseButton mouseButtonEvent) {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.IsReleased()) {
                EventBus.Instance.EmitEmptyRegionPressed();
                // if (InventoryManager.IsSelected() && InventoryManager.SelectedInventory.IsEditable) {
                //      DeleteItem();
                // }
            }
        }
    }
}