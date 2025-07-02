using Godot;
using RPG.global;
using RPG.scripts.inventory;
using RPG.ui.inventory;

namespace RPG.ui.item;

public partial class ItemSlot : InventorySlot {
    [Export] private Panel _borderHolder = null!;

    public override void Update(GizmoStack? pGizmoStack) {
        if (pGizmoStack?.Gizmo is null) {
            SetBorderColor(Colors.Black);
            IconHolder.Texture = null;
            TextHolder.Text = "";
        } else {
            SetBorderColor(pGizmoStack.Gizmo.GetRarityColor());
            IconHolder.Texture = pGizmoStack.Gizmo.GetCurrentIcon();
            if (pGizmoStack.Quantity <= 1) {
                TextHolder.Text = "";
            } else {
                TextHolder.Text = pGizmoStack.Quantity.ToString();
            }
        }
    }

    private void SetBorderColor(Color pColor) {
        StyleBox? stylebox = _borderHolder.GetThemeStylebox("panel");

        if (stylebox is not StyleBoxFlat styleBoxFlat) {
            Logger.UI.Error("Unhandled style box type.", true);
            return;
        }
        
        styleBoxFlat.BorderColor = pColor;
    }
}