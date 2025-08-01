using Godot;

namespace RPG.ui.debug;

public partial class Debug : CanvasLayer {
    public override void _Ready() {
        Visible = false;

        VisibilityChanged += () => {
            GetTree().Paused = Visible;
            // Reenable physics since SceneTree.Paused disables it...
            // Might need some kind of management of Physics' active state, though there's no way to query whether it's active or not...
            PhysicsServer3D.SetActive(true);

            if (Visible) {
                GetNode<DebugConsole>("Control/DebugConsole").GrabFocus();
            }
        };
    }

    public override void _Input(InputEvent pEvent) {
        if (pEvent is InputEventKey keyEvent) {
            if (keyEvent.Keycode == Key.Quoteleft && keyEvent.Pressed && keyEvent.GetModifiersMask() == 0) {
                Visible = !Visible;
                GetViewport().SetInputAsHandled();
            }
        }
    }
}