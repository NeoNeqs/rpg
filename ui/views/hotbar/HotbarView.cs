using Godot;
using RPG.global;
using RPG.scripts.inventory;
using EventBus = RPG.global.singletons.EventBus;
using ItemView = RPG.ui.views.item.ItemView;

namespace RPG.ui.views.hotbar;

public partial class HotbarView : ItemView {
    private static int _hotbarCounter;

    private int _hotbarId;

    private static readonly Key[] PrimaryKeys = [
        Key.Key1, Key.Key2, Key.Key3, Key.Key4, Key.Key5,
        Key.Q, Key.E, Key.R, Key.F, Key.G, Key.C, Key.X
    ];

    public override void _EnterTree() {
        _hotbarId = _hotbarCounter++;
    }

    public override void InitializeWith(Inventory? pInventory) {
        base.InitializeWith(pInventory);

        SetupDefaultKeybinds();
    }

    public override void _UnhandledKeyInput(InputEvent pEvent) {
        if (pEvent is not InputEventKey eventKey) {
            return;
        }

        if (pEvent.IsEcho()) {
            return;
        }

        if (pEvent.IsPressed()) {
            return;
        }

        if (Container is null) {
            return;
        }

        int size = Mathf.Min(Container.GetSize(), PrimaryKeys.Length);

        for (int i = 0; i < size; i++) {
            StringName action = new($"hotbar_{_hotbarId}_{i}");
            if (!InputMap.EventIsAction(eventKey, action, true)) {
                continue;
            }

            GizmoStack gizmoStack = Container.GetAt(i);
            if (gizmoStack.Gizmo is null) {
                break;
            }

            EventBus.Instance.EmitHotbarKeyPressed(gizmoStack.Gizmo);

            break;
        }
    }

    private void SetupDefaultKeybinds() {
        int modifierMask = _hotbarId;
#if TOOLS
        if (modifierMask > 0b111) {
            Logger.UI.Error($"Run out of possible modifiers fot hotbars! Max number of hotbars is 8!");
            return;
        }
#endif
        if (Container is null) {
            return;
        }
        
        int size = Mathf.Min(Container.GetSize(), PrimaryKeys.Length);

        for (int i = 0; i < size; i++) {
            StringName action = new($"hotbar_{_hotbarId}_{i}");
            if (!InputMap.HasAction(action)) {
                InputMap.AddAction(action);
                Logger.UI.Info($"Registered action '{action}' for hotbar id '{_hotbarId}'");
            }

            if (InputMap.ActionGetEvents(action).Count > 0) {
                continue;
            }

            InputMap.ActionAddEvent(action, new InputEventKey() {
                Keycode = PrimaryKeys[i],
                CtrlPressed = (modifierMask & 0b001) != 0,
                AltPressed = (modifierMask & 0b010) != 0,
                ShiftPressed = (modifierMask & 0b100) != 0
            });
        }
    }

    protected override void SetupHolder() { }
}