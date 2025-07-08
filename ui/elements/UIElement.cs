using Godot;

namespace RPG.ui.elements;

[GlobalClass]
public partial class UIElement : PanelContainer {
    [Export] public bool DragEnabled = true;

    private bool _dragEnabled = false;

    public override void _GuiInput(InputEvent pEvent) {
        if (!DragEnabled) {
            return;
        }

        switch (pEvent) {
            case InputEventMouseButton { ButtonIndex: MouseButton.Left } eventMouseButton: {
                _dragEnabled = eventMouseButton.IsPressed();

                break;
            }
            case InputEventMouseMotion eventMouseMotion when _dragEnabled:
                Position += eventMouseMotion.Relative;
                Position = Position.Clamp(Vector2.Zero, GetViewportRect().Size - Size);
                break;
        }
    }
}