using Godot;
using RPG.global;
using InventoryView = RPG.ui.views.inventory.InventoryView;

namespace RPG.ui.views.item;

public partial class ItemView : InventoryView {
    protected override void SetupHolder() {
        if (SlotHolder is not GridContainer gridContainer) {
            Logger.UI.Error($"Can't handle {SlotHolder} container type that is not a GridContainer", true);
            return;
        }

        gridContainer.Columns = GetInventory().Columns;
    }
}