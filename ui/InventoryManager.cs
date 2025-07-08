using Godot;
using RPG.global;
using RPG.global.singletons;
using RPG.scripts.combat;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;
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
        
#if DEBUG
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
        sv.GetInventory().SetAt(2, ResourceDB.GetSpell("spell:fire_bomb"));
        sv.GetInventory().HandleGizmoAction(0, hv.GetInventory(), 0, true);
        sv.GetInventory().HandleGizmoAction(1, hv.GetInventory(), 1, true);
#endif
    }

    private void CreateItemView(Inventory pInventory) {
        CreateInventoryView<ItemView>(pInventory, AssetDB.ItemView);
       
    }

    private void CreateSpellView(Inventory pInventory) {
        var t = CreateInventoryView<SpellView>(pInventory, AssetDB.SpellView);
        t.Position += new Vector2(0, 150);
    }

    private void CreateHotbarView(Inventory pInventory) {
        var view = CreateInventoryView<HotbarView>(pInventory, AssetDB.HotbarView);
        view.Position = new Vector2(400, 200);
    }

    // private void CreateStatView(Stats pStats, string pTitle) {
    //     var statView = AssetDB.StatView.Instantiate<StatView>();
    //
    //     statView.SetData(pStats, pTitle);
    //
    //     AddChild(statView);
    // }

    private void CreateEffectView(CombatManager pCombatManager) {
        var effectView = AssetDB.EffectView.Instantiate<EffectView>();
        effectView.InitializeWith(pCombatManager);
        
    }

    private T CreateInventoryView<T>(Inventory pInventory, PackedScene pViewScene) where T : InventoryView {
        var inventoryView = pViewScene.Instantiate<T>();

        inventoryView.InitializeWith(pInventory);
        inventoryView.Position = new Vector2(200, 200);
        inventoryView.SlotPressed += OnSlotPressed;
        inventoryView.SlotHovered += OnSlotHovered;
        inventoryView.SlotUnhovered += OnSlotUnhovered;
        AddChild(inventoryView);

        return inventoryView;
    }

    private void OnSlotPressed(View<GizmoStack> pSourceView, Slot pSlot, bool pIsRightClick) {
        // if (MouseStateMachine.CurrentState != MouseStateMachine.State.UIControl) {
        //     return;
        // }
        if (pSourceView is InventoryView inventoryView && pSlot is InventorySlot inventorySlot) {
            if (!IsSelected()) {
                Select(inventoryView, inventorySlot);
                return;
            }

            Inventory.ActionResult result = _selectedInventoryView!.GetInventory().HandleGizmoAction(
                _selectedSlotIndex,
                inventoryView.GetInventory(),
                pSlot.GetIndex(),
                pIsRightClick
            );

            switch (result) {
                case Inventory.ActionResult.None:
                    Unselect(inventoryView, inventorySlot);
                    break;
                case Inventory.ActionResult.Leftover:
                    GizmoStack selectedGizmoStack = _selectedInventoryView.GetInventory().GetAt(_selectedSlotIndex);
                    _dragSlot.Update(selectedGizmoStack);

                    GizmoStack pressedGizmoStack = inventoryView.GetInventory().GetAt(pSlot.GetIndex());
                    inventorySlot.Update(pressedGizmoStack);
                    break;
                default:
                    Logger.UI.Error($"Unhandled ActionResult {result.ToString()}.", true);
                    break;
            }
        } 
        else {
            Logger.UI.Error("null", true);
        } 
    }

    private void DeleteSelectedGizmo() {
        if (IsSelected()) {
            bool isDeleted = _selectedInventoryView!.GetInventory().Delete(_selectedSlotIndex);
            if (isDeleted) {
                Unselect(_selectedInventoryView!, _selectedInventoryView?.GetSlot<InventorySlot>(_selectedSlotIndex)!);
            }
        }
    }

    private void Select(InventoryView pSourceView, InventorySlot pSlot) {
        int slotIndex = pSlot.GetIndex();

        GizmoStack gizmoStack = pSourceView.GetInventory().GetAt(slotIndex);

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
            var selectedSlot = _selectedInventoryView?.GetSlot<InventorySlot>(_selectedSlotIndex);

            selectedSlot?.Unselect();

            GizmoStack gizmoStack = pSourceView.GetInventory().GetAt(pSlot.GetIndex());
            _tooltip.Update(gizmoStack);
            _dragSlot.Update(null);

            _selectedSlotIndex = -1;
            _selectedInventoryView = null;
        }
    }

    private void OnSlotHovered(View<GizmoStack> pSourceView, Slot pSlot) {
        if (pSourceView is not InventoryView inventoryView || pSlot is not InventorySlot inventorySlot) {
            Logger.UI.Error("null", true);
            return;
        }
        
        if (IsSelected()) {
            return;
        }

        _tooltip.Update(inventoryView.GetInventory().GetAt(pSlot.GetIndex()));
    }

    private void OnSlotUnhovered() {
        _tooltip.Update(null);
    }

    private bool IsSelected() {
        return _selectedSlotIndex != -1 && _selectedInventoryView != null;
    }
}