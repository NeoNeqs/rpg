using RPG.global;
using Godot;
using EventBus = RPG.global.singletons.EventBus;

namespace RPG.ui;

public partial class UI : Control {
    [Export] private InventoryManager _inventoryManager = null!;

    public override void _GuiInput(InputEvent pEvent) {
        if (pEvent is InputEventMouseButton mouseButtonEvent) {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.IsReleased()) {
                EventBus.Instance.EmitEmptyRegionPressed();
            }
        }
    }
}