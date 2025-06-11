using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using Godot.Collections;
using RPG.scripts.combat;
using RPG.scripts.inventory;

namespace RPG.world;

[GlobalClass]
public partial class Entity : Node3D {
    
    [Export] public CombatManager CombatManager = null!;
    [Export] public Inventory Armory = null!;
    [Export] public Inventory SpellBook = null!;
    [Export] public Stats BaseStats = null!;
    

    private PhysicsShapeQueryParameters3D _query = new();

    public override void _EnterTree() {
        _query.CollideWithBodies = true;
        _query.CollisionMask = int.MaxValue;
        _query.ShapeRid = PhysicsServer3D.SphereShapeCreate();
    }

    public override void _ExitTree() {
        PhysicsServer3D.FreeRid(_query.ShapeRid);
    }

    public override void _Ready() {
        Debug.Assert(GetChild(0) is PhysicsBody3D,
            $"The first child of {nameof(Entity)} should be a {nameof(PhysicsBody3D)}.");
        Debug.Assert(GetParent() is World, "All Entities should be a direct parent of World node.");
    }

    public List<Entity> GetEntitiesInRadius(float pRadius) {
        var result = new List<Entity>();

        _query.Transform = new Transform3D(new Basis(), GlobalTransform.Origin);
        _query.Exclude = [GetChild<PhysicsBody3D>(0).GetRid()];
        
        PhysicsServer3D.ShapeSetData(_query.ShapeRid, pRadius);

        Array<Dictionary> queryResults = GetWorld3D().DirectSpaceState.IntersectShape(_query);
        

        foreach (Dictionary queryResult in queryResults) {
            GodotObject collider = queryResult["collider"].AsGodotObject();
            
            // TODO: LoS checks. After we have all Entities in radius check if a raycast can hit that Entity. 
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