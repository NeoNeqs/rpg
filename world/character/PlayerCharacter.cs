using Godot;
using RPG.scripts.inventory;
using EventBus = RPG.global.singletons.EventBus;

namespace RPG.world.character;

[GlobalClass]
public partial class PlayerCharacter : entity.Entity {
    [Signal]
    public delegate void UserInputEventHandler();

    [Export] public required Inventory Inventory;

    // ReSharper disable once AsyncVoidMethod
    public override async void _Ready() {
        await ToSignal(GetTree().CurrentScene, Node.SignalName.Ready);
        
        EventBus.Instance.EmitCharacterInventoryLoaded(Inventory);
        EventBus.Instance.EmitCharacterSpellBookLoaded(SpellBook);
        CombatManager.TargetChanged += (entity.Entity _, entity.Entity? pNewEntity) => {
            EventBus.Instance.EmitPlayerTargetChanged(this, pNewEntity);
        };
    }

    public PlayerCharacterBody GetBody() {
        return GetChildOrNull<PlayerCharacterBody>(0);
    }
}