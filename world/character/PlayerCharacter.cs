using System.Threading.Tasks;
using RPG.global;
using Godot;
using RPG.scripts.effects;
using RPG.scripts.effects.components;
using RPG.scripts.inventory;
using SpellComponent = RPG.scripts.inventory.components.SpellComponent;

namespace RPG.world.character;

public partial class PlayerCharacter : Entity {
    [Signal]
    public delegate void MouseClickedEventHandler();

    public void OnMouseClicked(Vector3 pClickPosition) {
        // Oh-Oh Spaghetti-o
        if (!CombatManager.HasTarget()) {
            var dummyEntity = AssetDB.DummyEntity.Instantiate<Entity>();
            dummyEntity.GlobalPosition = pClickPosition;
            AddChild(dummyEntity);
            dummyEntity.QueueFree();

            CombatManager.TargetEntity = dummyEntity;
        }

        EmitSignalMouseClicked();
    }

    public async Task OnHotbarKeyPressed(Gizmo pGizmo) {
        var spellComponent = pGizmo.GetComponent<SpellComponent>();

        if (spellComponent is not null) {
            // TODO: move this to HotbarView, this should just be a flag that is set when item appears in the hotbar and
            //       removed when it is no more there. That is better then looping over effects every time key is pressed.

            foreach (Effect effect in spellComponent.Effects) {
                if (!effect.HasComponent<AreaEffectComponent>()) {
                    continue;
                }

                await ToSignal(this, SignalName.MouseClicked);
            }

            CombatManager.Cast(pGizmo);
        }

        // TODO: handle ItemComponent.use()
    }
}