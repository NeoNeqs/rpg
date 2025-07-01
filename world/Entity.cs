using System.Collections.Generic;
using global::RPG.global;
using Godot;
using Godot.Collections;
using RPG.scripts.combat;
using RPG.scripts.inventory;

namespace RPG.world;

[GlobalClass]
public partial class Entity : Node3D {
    [Export] public CombatManager CombatManager = null!;
    [Export] public Inventory? Armory;
    [Export] public Inventory SpellBook = null!;
    [Export] public Stats? BaseStats;

    public override void _Ready() {
        // Debug.Assert(GetChild(0) is PhysicsBody3D,
        //     $"The first child of {nameof(Entity)} should be a {nameof(PhysicsBody3D)}.");
        // Debug.Assert(GetParent() is World, "All Entities should be a direct parent of World node.");
    }

    public List<Entity> GetEntitiesInRadius(float pRadius) {
        var result = new List<Entity>();

        Array<Dictionary> queryResults = GetWorld().IntersectShape(
            GlobalTransform.Origin, 
            pRadius, 
            true, 
            2,
            GetChild<PhysicsBody3D>(0).GetRid()
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