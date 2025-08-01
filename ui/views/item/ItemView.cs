using global::RPG.global;
using Godot;
using RPG.ui.views.inventory;

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