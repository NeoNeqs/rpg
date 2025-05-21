using Godot;
using RPG.scripts.inventory;

namespace RPG.ui.inventory;

// TODO: this should extends ItemSlot
public partial class DragSlot : InventorySlot {
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