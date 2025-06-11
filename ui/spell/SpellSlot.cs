using RPG.scripts.inventory;
using RPG.ui.inventory;

namespace RPG.ui.spell;

public partial class SpellSlot : InventorySlot {
    public override void Update(GizmoStack? pGizmoStack) {
        if (pGizmoStack?.Gizmo is null) {
            IconHolder.Texture = null;
            TextHolder.Text = "";
        }
        else {
            IconHolder.Texture = pGizmoStack.Gizmo.Icon;
            TextHolder.Text = pGizmoStack.Gizmo.DisplayName;
        }
        
    }
}