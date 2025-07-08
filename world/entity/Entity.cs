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
    public delegate void BaseStatsAboutToChangeEventHandler(Stats? pOld, Stats? pNew);

    [Export] public CombatManager CombatManager = null!;
    [Export] public Inventory? Armory;
    [Export] public Inventory SpellBook = null!;

    [Export]
    public Stats? BaseStats {
        private set {
            // This setter will be called by the editor when the object is created (way before `_Ready` and `_EnterTree`)
            // So the 1 frame delay is hack to allow child nodes to connect to the signal below.
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

    // TODO: figure out how to make pExclude a default parameter
    public List<Entity> GetEntitiesInRadius(float pRadius, Array<Rid> pExclude) {
        var result = new List<Entity>();

        Array<Rid> exclude = [GetChild<PhysicsBody3D>(0).GetRid()];
        exclude.AddRange(pExclude);

        Array<Dictionary> queryResults = GetWorld().IntersectShape(
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