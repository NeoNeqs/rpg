using System.Threading.Tasks;
using RPG.global;
using Godot;
using Godot.Collections;
using RPG.scripts.effects;
using RPG.scripts.inventory;

namespace RPG.world.character;

// Oh-Oh Spaghetti-O
[GlobalClass]
public partial class PlayerCharacter : Entity {
    [Signal]
    public delegate void UserInputEventHandler();

    [Export] public required Inventory Inventory;
    private Decal decal => GetNodeOrNull<Decal>("Decal");
    private RayCast3D rayCast => decal.GetChild<RayCast3D>(0);

    private bool _shouldCast;
    private float _maxRange;

    // ReSharper disable once AsyncVoidMethod
    public override async void _Ready() {
        EventBus.Instance.HotbarKeyPressed += async (Gizmo pGizmo) => { await OnHotbarKeyPressed(pGizmo); };
        EventBus.Instance.EntitySelected += (Entity pEntity) => { CombatManager.TargetEntity = pEntity; };
        // IMPORTANT: `this._UnhandledInput` must only fire when specifically requested to not mess up _shouldCast state.
        SetProcessUnhandledInput(false);

        await ToSignal(GetTree().CurrentScene, Node.SignalName.Ready);
        EventBus.Instance.EmitCharacterInventoryLoaded(Inventory);
        EventBus.Instance.EmitCharacterSpellBookLoaded(SpellBook);
        
        decal.Visible = false;
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
        switch (pEvent) {
            case InputEventKey keyEvent when keyEvent.IsPressed() && keyEvent.Keycode == Key.Escape:
                CombatManager.TargetEntity = null;
                break;
        }
    }

    public override void _PhysicsProcess(double pDelta) {
        if (decal.Visible) {
            Vector3 playerPosition = GetBody().GlobalPosition;
            Vector3 toTarget = GetWorld().GetMouseWorldPosition(1_000_000) - playerPosition;
            if (toTarget.LengthSquared() > _maxRange * _maxRange) {
                toTarget = toTarget.Normalized() * _maxRange;
                Vector3 clamped = playerPosition + toTarget;
                clamped.Y = rayCast.GetCollisionPoint().Y;
                decal.GlobalPosition = clamped;
            } else {
                Vector3 clamped = playerPosition + toTarget;
                decal.GlobalPosition = clamped;
            }
        }
    }

    private async Task OnHotbarKeyPressed(Gizmo pGizmo) {
        (Array<Effect> aoeEffects, _maxRange) = pGizmo.GetAoeInfo();

        if (!CombatManager.HasTarget() && aoeEffects.Count != 0) {
            Vector3 mouseWorldPosition = GetWorld().GetMouseWorldPosition(_maxRange);
            if (mouseWorldPosition == Vector3.Inf) {
                return;
            }

            decal.GlobalPosition = mouseWorldPosition;
            decal.Visible = true;
            EventBus.Instance.EmitAoESelected(aoeEffects);
        
            SetProcessUnhandledInput(true);
            
            // This prevents casting spells e.g. when camera is being controlled.
            // Relies on CameraController releasing its state with at least 1 frame of delay
            do {
                await ToSignal(this, SignalName.UserInput);
            } while (MouseStateMachine.Instance.CurrentState != MouseStateMachine.State.Free);

            SetProcessUnhandledInput(false);

            decal.Visible = false;

            CombatManager.TargetEntity = GetWorld().CreateTempDummyEntity(mouseWorldPosition);
            if (_shouldCast) {
                CombatManager.Cast(pGizmo);
            }

            return;
        }

        CombatManager.Cast(pGizmo);
    }

    public PlayerCharacterBody GetBody() {
        return GetChildOrNull<PlayerCharacterBody>(0);
    }
}