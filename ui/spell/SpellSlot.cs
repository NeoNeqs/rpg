using Godot;
using RPG.scripts.inventory;
using RPG.ui.inventory;

namespace RPG.ui.spell;

[GlobalClass]
public partial class SpellSlot : InventorySlot {
    public override void Update(GizmoStack? pGizmoStack) {
        base.Update(pGizmoStack);
        if (pGizmoStack?.Gizmo is null) {
            IconHolder.Texture = null;
            TextHolder.Text = "";
        } else {
            IconHolder.Texture = pGizmoStack.Gizmo.GetCurrentIcon();
            TextHolder.Text = pGizmoStack.Gizmo.GetCurrentDisplayName();
        }
    }
}