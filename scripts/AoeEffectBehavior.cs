using Godot;
using RPG.scripts.combat;
using RPG.scripts.inventory.components;
using RPG.world;
using RPG.world.character;
using RPG.world.entity;

namespace RPG.scripts;

[Tool, GlobalClass]
public partial class AoeEffectBehavior : EffectBehavior {
    [Export] public float Radius { private set; get; }

    public override void Run(Entity pSource, AppliedEffect pAppliedEffect, Entity pTarget, bool pIsHarmful) {
        World world = pSource.GetWorld();
        SpellComponent spellCmp = pAppliedEffect.EffectSource.GetComponent<SpellComponent, SequenceSpellComponent>()!;

        Vector3 bodyPosition = ((PlayerCharacter)pSource).GetBody().GlobalPosition;
        Vector3 mousePos = world.GetMouseWorldPosition(bodyPosition, spellCmp.GetRange(), 0b11);

        world.CreateAoe(
            mousePos,
            Radius,
            pAppliedEffect.Effect,
            (Entity pEnteredEntity) => {
                GD.Print($"{pEnteredEntity.Name} Entered");
                pEnteredEntity.SpellManager.ApplyEffect(pAppliedEffect.EffectSource, pAppliedEffect.Effect, pSource,
                    pIsHarmful);
            },
            (Entity pExitedEntity) => { GD.Print($"{pExitedEntity.Name} Exited"); }
        );


        if (!pAppliedEffect.Effect.Timer.IsInsideTree()) {
            world.AddChild(pAppliedEffect.Effect.Timer);
        }
    }
}