using Godot;
using RPG.scripts.inventory;
using ItemSlot = RPG.ui.views.item.ItemSlot;

namespace RPG.ui;

[GlobalClass]
public partial class DragSlot : ItemSlot {
    public override void _Process(double pDelta) {
        GlobalPosition = GetViewport().GetMousePosition();
        GlobalPosition = GlobalPosition.Clamp(Vector2.Zero, GetViewportRect().Size - Size * Scale);
    }

    public override void Update(GizmoStack? pGizmoStack) {
        if (pGizmoStack is null) {
            Visible = false;
        } else {
            Visible = true;
            base.Update(pGizmoStack);
        }
        
        SetProcess(Visible);
    }
}