using Godot;
using RPG.scripts.effects;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;
using RPG.world;
using RPG.world.character;
using RPG.world.entity;

namespace RPG.scripts;

[Tool, GlobalClass]
public partial class AoeEffectBehavior : EffectBehavior {
    [Export] public float Radius { private set; get; }

    public override void Run(Entity pSourceEntity, Gizmo pEffectOwner, Effect pEffect, Entity pTargetEntity) {
        World world = pSourceEntity.GetWorld();
        SpellComponent spellComponent = pEffectOwner.GetComponent<SpellComponent, ChainSpellComponent>()!;
        Vector3 bodyPosition = ((PlayerCharacter)pSourceEntity).GetBody().GlobalPosition;
        Vector3 mousePos = world.GetMouseWorldPosition(bodyPosition, spellComponent.GetRange(), 0b11);
        
        world.CreateAoe(
            mousePos,
            Radius,
            pEffect,
            (Entity pEnteredEntity) => {
                GD.Print($"{pEnteredEntity.Name} Entered");
                pEnteredEntity.SpellManager.ApplyEffect(pEffectOwner, pEffect, pSourceEntity.SpellManager);
            },
            (Entity pExitedEntity) => {
                GD.Print($"{pExitedEntity.Name} Exited");
            }
        );
  

        if (!pEffect.Timer.IsInsideTree()) {
            world.AddChild(pEffect.Timer);
        }
    }
}