using global::RPG.global;
using Godot;
using Godot.Collections;

namespace RPG.world.entity;

[Tool]
public partial class ModelHolder : Node3D {
    private static readonly Shape3D Shape = GD.Load<Shape3D>("uid://cx78nl58jcwlj");

    private Node3D? _currentModel;
    [Export] public PackedScene? ModelScene;

    public override void _EnterTree() {
        if (ModelScene is null) {
            return;
        }

        if (_currentModel is not null) {
            RemoveChild(_currentModel);
            _currentModel.QueueFree();
        }

        _currentModel = ModelScene.Instantiate<Node3D>();
        _currentModel.Rotation = new Vector3(0.0f, -float.Pi / 2.0f, 0.0f);
        _currentModel.Scale = new Vector3(0.2f, 0.2f, 0.2f);
        this.AddChildOwned(_currentModel, this);
        _currentModel.Name = "Model";
    }


    public Array<Entity> GetEntitiesInSight(uint pCollisionMask = 0b10) {
        var query = new PhysicsShapeQueryParameters3D {
            ShapeRid = Shape.GetRid(),
            Transform = GlobalTransform,
            CollisionMask = pCollisionMask,
            CollideWithBodies = true,
            CollideWithAreas = false
        };

        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
        Array<Dictionary> results = spaceState.IntersectShape(query);

        var entities = new Array<Entity>();

        foreach (Dictionary result in results) {
            GodotObject collider = result["collider"].AsGodotObject();

            if (collider is PhysicsBody3D body && body.GetParent() is Entity ent) {
                entities.Add(ent);
            }
        }

        return entities;
    }
}