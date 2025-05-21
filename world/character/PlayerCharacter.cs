using System.Threading.Tasks;
using RPG.global;
using Godot;
using Godot.Collections;
using RPG.scripts.effects;
using RPG.scripts.effects.components;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;

namespace RPG.world.character;

// Oh-Oh Spaghetti-O
public partial class PlayerCharacter : Entity {
    [Signal]
    public delegate void UserInputEventHandler();
    
    [Export]
    private Inventory _inventory = null!;

    private bool _shouldCast = true;

    // ReSharper disable once AsyncVoidMethod
    public override async void _Ready() {
        EventBus.Instance.HotbarKeyPressed += async (Gizmo pGizmo) => { await OnHotbarKeyPressed(pGizmo); };
        // IMPORTANT: `this._UnhandledInput` must only fire when specifically requested to not mess up _shouldCast state.
        SetProcessUnhandledInput(false);
        
        await ToSignal(GetTree().CurrentScene, Node.SignalName.Ready);
        EventBus.Instance.EmitCharacterInventoryLoaded(_inventory);
    }

    // public override void _PhysicsProcess(double delta) {
    //     GD.Print("-----------------------------------------");
    //     GD.Print(_inventory.At(0).Gizmo);
    //     GD.Print(_inventory.At(1).Gizmo);
    //     GD.Print(_inventory.At(2).Gizmo);
    //     GD.Print(_inventory.At(3).Gizmo);
    //     GD.Print("-----------------------------------------");
    // }

    public override void _UnhandledInput(InputEvent pEvent) {
        _shouldCast = pEvent switch {
            InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } => true,
            _ => false
        };
        EmitSignalUserInput();
    }

    private async Task OnHotbarKeyPressed(Gizmo pGizmo) {
        SpellComponent? spellComponent = pGizmo.GetComponent<SpellComponent, ChainSpellComponent>();

        if (spellComponent is null) {
            Logger.Core.Info("Unable to cast a spell without a SpellComponent");
            return;
        }

        Array<AreaOfEffectComponent> AoEs = [];
        float maxEffectDistance = float.NegativeInfinity;
        // AreaOfEffectComponent? firstAreaEffect = null;
        // Figure out if any of effects are AoE
        foreach (Effect effect in spellComponent.Effects) {
            if (!effect.HasComponent<AreaOfEffectComponent>()) {
                continue;
            }
            
            AreaOfEffectComponent? aoe = effect.GetComponent<AreaOfEffectComponent>();
            if (aoe is not null) {
                AoEs.Add(aoe);
                if (aoe.MaxDistance > maxEffectDistance) {
                    maxEffectDistance = aoe.MaxDistance;
                }
            }

            break;
        }

        if (_shouldCast && CombatManager.TargetEntity is null && AoEs.Count != 0) {
            EventBus.Instance.EmitAoESelected(AoEs);
            // wait for the user input from `this._UnhandledInput()`
            SetProcessUnhandledInput(true);
            await ToSignal(this, SignalName.UserInput);
            SetProcessUnhandledInput(false);

            Vector3 mouseWorldPosition = GetWorld().GetMouseWorldPosition(maxEffectDistance);
            if (mouseWorldPosition == Vector3.Inf) {
                _shouldCast = true;
                return;
            }

            CombatManager.TargetEntity = GetWorld().CreateDummyEntity(mouseWorldPosition);
        }

        if (_shouldCast) {
            CombatManager.Cast(pGizmo);
        }

        _shouldCast = true;
    }
}