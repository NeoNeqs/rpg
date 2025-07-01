using Godot;
using RPG.global;
using RPG.scripts.combat;
using RPG.scripts.inventory;
using RPG.ui.attributes;
using RPG.ui.hotbar;
using RPG.ui.inventory;
using RPG.ui.item;
using RPG.ui.spell;
using EventBus = RPG.global.singletons.EventBus;
using MouseStateMachine = RPG.global.singletons.MouseStateMachine;

namespace RPG.ui;

[GlobalClass]
public partial class InventoryManager : Control {
    [Export] private Tooltip _tooltip = null!;
    [Export] private DragSlot _dragSlot = null!;

    private int _selectedSlotIndex = -1;
    private InventoryView? _selectedInventoryView = null;


    // ReSharper disable once AsyncVoidMethod
    // Can't make _Ready return `Task` since it's a virtual function :/
    public override async void _Ready() {
        EventBus.Instance.CharacterInventoryLoaded += CreateItemView;
        EventBus.Instance.CharacterSpellBookLoaded += CreateSpellView;
        // EventBus.Instance.CharacterStatsLoaded += 
        EventBus.Instance.EmptyRegionPressed += DeleteSelectedGizmo;
        CreateHotbarView(GD.Load<Inventory>("uid://b1v0bkq8bhhic"));

#if TOOLS
        await ToSignal(EventBus.Instance, EventBus.SignalName.CharacterSpellBookLoaded);
        SpellView sv = null;
        HotbarView hv = null;
        foreach (Node child in GetChildren()) {
            switch (child) {
                case SpellView view:
                    sv = view;
                    break;
                case HotbarView hotbarView:
                    hv = hotbarView;
                    break;
            }
        }

        sv.Inventory.HandleGizmoAction(0, hv.Inventory, 0, true);
        sv.Inventory.HandleGizmoAction(1, hv.Inventory, 1, true);
#endif
    }

    private void CreateItemView(Inventory pInventory) {
        CreateInventoryView<ItemView>(pInventory, AssetDB.ItemView);
    }

    private void CreateSpellView(Inventory pInventory) {
        CreateInventoryView<SpellView>(pInventory, AssetDB.SpellView);
    }

    private void CreateHotbarView(Inventory pInventory) {
        var view = CreateInventoryView<HotbarView>(pInventory, AssetDB.HotbarView);
        view.Position = new Vector2(400, 200);
    }

    private void CreateStatView(Stats pStats, string pTitle) {
        StatView statView = AssetDB.StatView.Instantiate<StatView>();

        statView.SetData(pStats, pTitle);

        AddChild(statView);
    }

    private T CreateInventoryView<T>(Inventory pInventory, PackedScene pViewScene) where T : InventoryView {
        T? itemView = pViewScene.Instantiate<T>();

        itemView.SetData(pInventory);
        itemView.Position = new Vector2(200, 200);
        itemView.SlotPressed += OnSlotPressed;
        itemView.SlotHovered += OnSlotHovered;
        itemView.SlotUnhovered += OnSlotUnhovered;
        AddChild(itemView);

        return itemView;
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
                GizmoStack selectedGizmoStack = _selectedInventoryView.Inventory.GetAt(_selectedSlotIndex);
                _dragSlot.Update(selectedGizmoStack);

                GizmoStack pressedGizmoStack = pSourceView.Inventory.GetAt(pSlot.GetIndex());
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

        GizmoStack gizmoStack = pSourceView.Inventory.GetAt(slotIndex);

        if (gizmoStack.Quantity == 0) {
            return;
        }

        if (MouseStateMachine.Instance.RequestState(MouseStateMachine.State.InventoryControl)) {
            _dragSlot.Update(gizmoStack);
            _tooltip.Update(null);
            pSlot.Select();

            _selectedSlotIndex = slotIndex;
            _selectedInventoryView = pSourceView;
        }
    }

    private void Unselect(InventoryView pSourceView, InventorySlot pSlot) {
        if (MouseStateMachine.Instance.RequestState(MouseStateMachine.State.Free)) {
            InventorySlot? selectedSlot = _selectedInventoryView?.GetSlot(_selectedSlotIndex);

            selectedSlot?.Unselect();
            // selectedSlot.Update(_selectedInventoryView?.Inventory.At(_selectedSlotIndex));

            GizmoStack gizmoStack = pSourceView.Inventory.GetAt(pSlot.GetIndex());
            _tooltip.Update(gizmoStack);
            _dragSlot.Update(null);

            _selectedSlotIndex = -1;
            _selectedInventoryView = null;
        }
    }

    private void OnSlotHovered(InventoryView pSourceView, InventorySlot pSlot) {
        if (IsSelected()) {
            return;
        }

        _tooltip.Update(pSourceView.Inventory.GetAt(pSlot.GetIndex()));
    }

    private void OnSlotUnhovered() {
        _tooltip.Update(null);
    }

    private bool IsSelected() {
        return _selectedSlotIndex != -1 && _selectedInventoryView != null;
    }
}