using Godot;
using EventBus = RPG.global.singletons.EventBus;

namespace RPG.ui;

public partial class UI : Control {
    [Export] private InventoryManager _inventoryManager = null!;

    public override void _Ready() {
// #if DEBUG
//         Node? debugNode = GD.Load<PackedScene>("uid://dkofbfjjtmkm6").Instantiate();
//         GetTree().Root.AddChild(debugNode);
// #endif
    }

    public override void _PhysicsProcess(double pDelta) {
        GetNodeOrNull<Label>("FPSLabel").Text = $"{Engine.GetFramesPerSecond()}";
    }

    public override void _GuiInput(InputEvent pEvent) {
        if (pEvent is InputEventMouseButton mouseButtonEvent) {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.IsReleased()) {
                EventBus.Instance.EmitEmptyRegionPressed();
            }
        }
    }
}