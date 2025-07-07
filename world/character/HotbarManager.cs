using System.Threading.Tasks;
using RPG.global;
using Godot;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;
using EventBus = RPG.global.singletons.EventBus;
using MouseStateMachine = RPG.global.singletons.MouseStateMachine;

namespace RPG.world.character;

[GlobalClass]
public partial class HotbarManager : Node {
    [Signal]
    public delegate void UserInputEventHandler();

    private AoeDecal Decal => GetNode<AoeDecal>("AoeDecal");

    private bool _shouldCast;
    private bool _waiting;

    public override void _Ready() {
        // NOTE: Those events are player specific and are not triggered by NPC (other Entities).
        // This means those events can't be moved inside CombatManager, since all entities have the CombatManager class.
        EventBus.Instance.HotbarKeyPressed += async (Gizmo pGizmo) => { await OnHotbarKeyPressed(pGizmo); };
        
        // IMPORTANT: this connection must be here and not in CombatManager, because selecting entities with mouse is an action
        //            that only player can do.
        EventBus.Instance.EntitySelected += (Entity pEntity) => {
            GetCharacter().CombatManager.TargetEntity = pEntity;
        };

        // NOTE: `this._UnhandledInput` must only fire when specifically requested to not mess up _shouldCast state.
        SetProcessUnhandledInput(false);
    }

    private async Task OnHotbarKeyPressed(Gizmo pGizmo) {
        if (_waiting) {
            _waiting = false;
            EmitSignalUserInput();
        }

        PlayerCharacter character = GetCharacter();
        SpellComponent? spellComponent = pGizmo.GetComponent<SpellComponent, ChainSpellComponent>();

        if (spellComponent is null) {
            return;
        }

        if (spellComponent.IsOnCooldown()) {
            return;
        }
    
        if (spellComponent.IsAoe()) {
            Vector3 mouseWorldPosition = character.GetWorld().GetMouseWorldPosition(spellComponent.GetRange());
            if (mouseWorldPosition == Vector3.Inf) {
                return;
            }

            // Go back to calling AoeDecal.Update() manually here...
            Decal.Update(mouseWorldPosition, spellComponent.GetRange());
            SetProcessUnhandledInput(true);

            _waiting = true;
            // This prevents casting spells e.g. when camera is being controlled.
            // Relies on CameraController releasing its state with at least 1 frame of delay
            do {
                await ToSignal(this, SignalName.UserInput);
            } while (MouseStateMachine.Instance.CurrentState != MouseStateMachine.State.Free && _waiting);

            _waiting = false;

            SetProcessUnhandledInput(false);
            Decal.Update(Vector3.Inf, 0);

            Entity dummyTarget = character.GetWorld().CreateTempDummyEntity(Decal.GlobalPosition);
            if (_shouldCast) {
                character.CombatManager.Cast(pGizmo, dummyTarget);
            }

            return;
        }

        character.CombatManager.Cast(pGizmo, null);
    }

    public override void _UnhandledInput(InputEvent pEvent) {
        switch (pEvent) {
            // Important: Most user actions must be triggered when action is released and NOT pressed!
            case InputEventMouseButton mouseEvent
                when pEvent.IsReleased() && mouseEvent.ButtonIndex == MouseButton.Left:
                _shouldCast = true;
                EmitSignalUserInput();
                break;
            case InputEventMouseButton mouseEvent2
                when pEvent.IsReleased() && mouseEvent2.ButtonIndex == MouseButton.Right:

                _shouldCast = false;
                EmitSignalUserInput();
                break;
        }
    }

    public override void _UnhandledKeyInput(InputEvent pEvent) {
        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        switch (pEvent) {
            case InputEventKey keyEvent when keyEvent.IsPressed() && keyEvent.Keycode == Key.Escape:
                GetCharacter().CombatManager.TargetEntity = null;
                break;
        }
    }

    private PlayerCharacter GetCharacter() {
        return GetParentOrNull<PlayerCharacter>();
    }
}