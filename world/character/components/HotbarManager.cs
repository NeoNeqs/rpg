using System.Threading.Tasks;
using global::RPG.global;
using Godot;
using RPG.global.enums;
using RPG.global.singletons;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;
using RPG.world.entity;

namespace RPG.world.character.components;

[GlobalClass]
public partial class HotbarManager : Node {
    [Signal]
    public delegate void UserInputEventHandler();

    private bool _castCanceled;
    private bool _waiting;

    public override void _Ready() {
        // NOTE: Those events are player specific and are not triggered by NPC (other Entities).
        // This means those events can't be moved inside CombatManager, since all entities have the CombatManager class.
        EventBus.Instance.HotbarKeyPressed += async (Gizmo pGizmo) => { await OnHotbarKeyPressed(pGizmo); };

        // IMPORTANT: this connection must be here and not in CombatManager, because selecting entities with mouse is an action
        //            that only player can do.
        EventBus.Instance.EntitySelected += (Entity pEntity) => { GetCharacter().SpellManager.TargetEntity = pEntity; };

        // NOTE: `this._UnhandledInput` must only fire when specifically requested to not mess up _shouldCast state.
        SetProcessUnhandledInput(false);
    }

    private async Task OnHotbarKeyPressed(Gizmo pGizmo) {
        if (_waiting) {
            _waiting = false;
            EmitSignalUserInput();
            return;
        }

        PlayerCharacter character = GetCharacter();
        SpellComponent? spellComponent = pGizmo.GetComponent<SpellComponent, SequenceSpellComponent>();

        if (spellComponent is null || spellComponent.IsOnCooldown()) {
            return;
        }

        if (spellComponent.IsAoe()) {
            AoeDecal decal = GetCharacter().GetWorld().GetDecal();

            await decal.Update(spellComponent.GetRange());
            SetProcessUnhandledInput(true);

            _waiting = true;
            // This prevents casting spells e.g. when camera is being controlled.
            // Relies on CameraController releasing its state with at least 1 frame of delay
            do {
                await ToSignal(this, SignalName.UserInput);
            } while (MouseStateMachine.Instance.CurrentState != MouseStateMachine.State.Free && _waiting);

            _waiting = false;

            SetProcessUnhandledInput(false);

            decal.Disable();
        }

        if (spellComponent.CastTimeSeconds > 0) {
            await ToSignal(GetTree().CreateTimer(spellComponent.CastTimeSeconds), Timer.SignalName.Timeout);
        }

        if (_castCanceled) {
            return;
        }

        CastResult castResult = character.SpellManager.CastNoCDCheck(pGizmo, spellComponent);

        switch (castResult) {
            // case CastResult.WrongTarget:
            // Logger.Combat.Info("Can't attack that target!");
            // break;
            case CastResult.NoValidTarget:
                Logger.Combat.Critical("Unable to attain a valid target for the spell", true);
                break;
            case CastResult.Ok:
            case CastResult.NoSpellComponent:
            case CastResult.OnCooldown:
            default:
                break;
        }
    }

    public override void _UnhandledInput(InputEvent pEvent) {
        switch (pEvent) {
            // Important: Most user actions must be triggered when action is released and NOT pressed!
            case InputEventMouseButton mouseEvent
                when pEvent.IsReleased() && mouseEvent.ButtonIndex == MouseButton.Left:
                _castCanceled = false;
                EmitSignalUserInput();
                break;
            case InputEventMouseButton mouseEvent2
                when pEvent.IsReleased() && mouseEvent2.ButtonIndex == MouseButton.Right:
            case InputEventKey keyEvent when keyEvent.IsPressed() && keyEvent.Keycode == Key.Escape:

                _castCanceled = true;
                EmitSignalUserInput();
                break;
        }
    }

    public override void _UnhandledKeyInput(InputEvent pEvent) {
        // ReSharper disable once ConvertSwitchStatementToSwitchExpression
        switch (pEvent) {
            case InputEventKey keyEvent when keyEvent.IsPressed() && keyEvent.Keycode == Key.Escape:
                GetCharacter().SpellManager.TargetEntity = null;
                break;
        }
    }

    private PlayerCharacter GetCharacter() {
        return GetParentOrNull<PlayerCharacter>();
    }
}