using System.Collections.Generic;
using Godot;
using Godot.Collections;
using RPG.global.enums;
using RPG.scripts.combat;
using RPG.scripts.inventory;

namespace RPG.world.entity;

[GlobalClass]
public partial class Entity : Node3D {
    [Signal]
    public delegate void AboutToBeFreedEventHandler();

    [Signal]
    public delegate void BaseStatsAboutToChangeEventHandler(Stats? pOld, Stats? pNew);

    [Export] public SpellManager SpellManager = null!;
    [Export] public Inventory? Armory;
    [Export] public Inventory SpellBook = null!;

    [Export]
    public Stats? BaseStats {
        private set {
            // TODO: check if this works:
            
            // This setter will be called by the editor when the object is created (way before `_Ready` and `_EnterTree`)
            // So the delay is a hack to allow child nodes to connect to the signal below.
            CallDeferred(nameof(EmitBaseStatsAboutToChanged), Variant.From(_baseStats), Variant.From(value));
            _baseStats = value;
        }
        get => _baseStats;
    }

    [Export] public Faction Faction;

    private Stats? _baseStats;

    private void EmitBaseStatsAboutToChanged(Stats? pOld, Stats? pNew) {
        EmitSignalBaseStatsAboutToChange(pOld, pNew);
    }

    public override void _EnterTree() {
        TreeExiting += () => {
            if (IsQueuedForDeletion()) {
                EmitSignalAboutToBeFreed();
            }
        };
    }

    // Move to world
    public List<Entity> GetEntitiesInRadius(float pRadius, Array<Rid> pExclude) {
        var result = new List<Entity>();

        Array<Rid> exclude = [GetChild<PhysicsBody3D>(0).GetRid()];
        exclude.AddRange(pExclude);

        Array<Dictionary> queryResults = GetWorld().IntersectSphere(
            GlobalTransform.Origin,
            pRadius,
            true,
            2,
            exclude
        );

        foreach (Dictionary queryResult in queryResults) {
            GodotObject collider = queryResult["collider"].AsGodotObject();

            // TODO: Add LoS checks. After we have all Entities in radius check if a raycast can hit that Entity. 
            //       Only then add it.

            if (collider is PhysicsBody3D body && body.GetParent() is Entity ent && ent != this) {
                result.Add(ent);
            }
        }

        return result;
    }

    public World GetWorld() {
        return GetParent<World>();
    }
}