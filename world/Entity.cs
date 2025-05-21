using System.Collections.Generic;
using Godot;
using Godot.Collections;
using RPG.scripts.combat;
using RPG.scripts.inventory;

namespace RPG.world;

public partial class Entity : Node3D {
    [Export] public CombatManager CombatManager = null!;
    [Export] public Inventory Armory = null!;
    [Export] public Attributes BaseAttributes = null!;


    public List<Entity> GetEntitiesInRadius(float pRadius) {
        var result = new List<Entity>();

        // FIXME: This will probably not work. 
        var area = new Area3D() { Monitoring = true, Monitorable = false, InputRayPickable = false };
        var collision = new CollisionShape3D() { Shape = new SphereShape3D() { Radius = pRadius } };
        area.AddChild(collision);
        AddChild(area);

        foreach (Node3D node in area.GetOverlappingBodies()) {
            if (node is Entity entity) {
                result.Add(entity);
            }
        }

        area.QueueFree();

        return result;
    }
}