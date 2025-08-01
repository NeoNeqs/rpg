using Godot;

namespace RPG.ui.debug;

public partial class DebugControl : Control {
    //public override void _GuiInput(InputEvent pEvent) {
    //     if (pEvent is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Middle) {
    //         if (GetTree().CurrentScene is not Main main) {
    //             Logger.Core.Error($"Current scene is not {nameof(Main)}.");
    //             return;
    //         }
    //
    //         World world = main.GetWorld();
    //
    //         Dictionary result = world.IntersectRay(1_000.0f, uint.MaxValue);
    //
    //         if (result.Count == 0) {
    //             Logger.Core.Info("No object found at the mouse location.");
    //             return;
    //         }
    //
    //         GodotObject obj = result["collider"].AsGodotObject();
    //         var tree = GetNode<ObjectBrowser>("UIElement/VBoxContainer/Tree");
    //         tree.GetRoot().RemoveChild(tree.GetRoot().GetChild(0));
    //         tree.CreateObjectProperty(tree.GetRoot(), obj.GetType().Name, obj);
    //     }
    // }
}