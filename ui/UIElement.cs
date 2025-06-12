using Godot;

namespace RPG.ui;

[GlobalClass]
public partial class UIElement : PanelContainer {
    private bool _dragEnabled = false;

    public override void _GuiInput(InputEvent pEvent) {
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