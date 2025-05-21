using System.Threading.Tasks;
using Godot;
using RPG.global;
using RPG.scripts.combat;
using RPG.scripts.inventory;
using RPG.ui.attributes;
using RPG.ui.hotbar;
using RPG.ui.inventory;
using RPG.ui.item;
using RPG.ui.spell;

namespace RPG.ui;

[GlobalClass]
public partial class InventoryManager : Control {
    [Export] private Tooltip _tooltip = null!;
    [Export] private DragSlot _dragSlot = null!;

    private int _selectedSlotIndex = -1;
    private InventoryView? _selectedInventoryView = null;

    public override void _EnterTree() {
        EventBus.Instance.CharacterInventoryLoaded += CreateItemView;
        
        // EventBus.Instance.CharacterStatsLoaded += 
        EventBus.Instance.EmptyRegionPressed += DeleteSelectedGizmo;
    }

    private void CreateItemView(Inventory pInventory) {
        CreateInventoryView<ItemView>(pInventory, AssetDB.ItemView);
    }

    private void CreateSpellView(Inventory pInventory) {
        CreateInventoryView<SpellView>(pInventory, AssetDB.SpellView);
    }

    private void CreateHotbarView(Inventory pInventory) {
        CreateInventoryView<HotbarView>(pInventory, AssetDB.HotbarView);
    }

    private void CreateStatView(Stats pStats, string pTitle) {
        StatView statView = AssetDB.StatView.Instantiate<StatView>();

        statView.SetData(pStats, pTitle);

        AddChild(statView);
    }

    private void CreateInventoryView<T>(Inventory pInventory, PackedScene pViewScene) where T : InventoryView {
        T? itemView = pViewScene.Instantiate<T>();

        itemView.SetData(pInventory);
        itemView.Position = new Vector2(200, 200);
        itemView.SlotPressed += OnSlotPressed;
        itemView.SlotHovered += OnSlotHovered;
        itemView.SlotUnhovered += OnSlotUnhovered;
        AddChild(itemView);
    }

    private void OnSlotPressed(InventoryView pSourceView, InventorySlot pSlot, bool pIsRightClick) {
        // if (MouseStateMachine.CurrentState != MouseStateMachine.State.UIControl) {
        //     return;
        // }

        if (!IsSelected()) {
            Select(pSourceView, pSlot);
            return;
        }
        
        Inventory.ActionResult result = _selectedInventoryView!.Inventory.HandleGizmoAction(
            _selectedSlotIndex,
            pSourceView.Inventory,
            pSlot.GetIndex(),
            pIsRightClick
        );

        switch (result) {
            case Inventory.ActionResult.None:
                Unselect(pSourceView, pSlot);
                break;
            case Inventory.ActionResult.Leftover:
                GizmoStack selectedGizmoStack = _selectedInventoryView.Inventory.At(_selectedSlotIndex);
                _dragSlot.Update(selectedGizmoStack);

                GizmoStack pressedGizmoStack = pSourceView.Inventory.At(pSlot.GetIndex());
                pSlot.Update(pressedGizmoStack);
                break;
            default:
                Logger.UI.Error($"Unhandled ActionResult {result.ToString()}.", true);
                break;
        }
    }

    private void DeleteSelectedGizmo() {
        if (IsSelected()) {
            bool isDeleted = _selectedInventoryView!.Inventory.Delete(_selectedSlotIndex);
            if (isDeleted) {
                Unselect(_selectedInventoryView!, _selectedInventoryView?.GetSlot(_selectedSlotIndex)!);
            }
        }
    }

    private void Select(InventoryView pSourceView, InventorySlot pSlot) {
        int slotIndex = pSlot.GetIndex();

        GizmoStack gizmoStack = pSourceView.Inventory.At(slotIndex);

        if (gizmoStack.Quantity == 0) {
            return;
        }

        MouseStateMachine.SetState(MouseStateMachine.State.InventoryControl);
        _dragSlot.Update(gizmoStack);
        _tooltip.Update(null);
        pSlot.Select();

        _selectedSlotIndex = slotIndex;
        _selectedInventoryView = pSourceView;
    }

    private void Unselect(InventoryView pSourceView, InventorySlot pSlot) {
        MouseStateMachine.SetState(MouseStateMachine.State.UIControl);
        InventorySlot? selectedSlot = _selectedInventoryView?.GetSlot(_selectedSlotIndex);

        selectedSlot?.Unselect();
            // selectedSlot.Update(_selectedInventoryView?.Inventory.At(_selectedSlotIndex));

        GizmoStack gizmoStack = pSourceView.Inventory.At(pSlot.GetIndex());
        _tooltip.Update(gizmoStack);
        _dragSlot.Update(null);

        _selectedSlotIndex = -1;
        _selectedInventoryView = null;
    }

    private void OnSlotHovered(InventoryView pSourceView, InventorySlot pSlot) {
        if (IsSelected()) {
            return;
        }

        _tooltip.Update(pSourceView.Inventory.At(pSlot.GetIndex()));
    }

    private void OnSlotUnhovered() {
        _tooltip.Update(null);
    }

    private bool IsSelected() {
        return _selectedSlotIndex != -1 && _selectedInventoryView != null;
    }
}