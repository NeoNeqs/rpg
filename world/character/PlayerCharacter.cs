using Godot;
using RPG.global.singletons;
using RPG.scripts.inventory;
using RPG.world.entity;

namespace RPG.world.character;

[GlobalClass]
public partial class PlayerCharacter : Entity {
    [Signal]
    public delegate void UserInputEventHandler();

    [Export] public required Inventory Inventory;

    // ReSharper disable once AsyncVoidMethod
    public override async void _Ready() {
        await ToSignal(GetTree().CurrentScene, Node.SignalName.Ready);

        EventBus.Instance.EmitCharacterInventoryLoaded(Inventory);
        EventBus.Instance.EmitCharacterSpellBookLoaded(SpellBook);
        SpellManager.TargetChanged += (Entity _, Entity? pNewEntity) => {
            EventBus.Instance.EmitPlayerTargetChanged(this, pNewEntity);
        };
        base._Ready();
    }

    public PlayerCharacterBody GetBody() {
        return GetChildOrNull<PlayerCharacterBody>(0);
    }
}