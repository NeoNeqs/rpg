using RPG.global;
using Godot;
using Godot.Collections;

namespace RPG.world;

[GlobalClass]
public partial class World : Node3D {
    private readonly PhysicsShapeQueryParameters3D _shapeQuery = new();
    private readonly PhysicsRayQueryParameters3D _rayQuery = new();

    public override void _EnterTree() {
        _shapeQuery.CollideWithBodies = true;
        _shapeQuery.CollisionMask = int.MaxValue;
        _shapeQuery.ShapeRid = PhysicsServer3D.SphereShapeCreate();

        _rayQuery.CollideWithBodies = true;
        _rayQuery.CollisionMask = int.MaxValue;
    }

    public override void _ExitTree() {
        PhysicsServer3D.FreeRid(_shapeQuery.ShapeRid);
    }

    public Vector3 GetMouseWorldPosition(float pMaxLength) {
        if (pMaxLength <= 0) {
            Logger.Core.Critical($"{nameof(pMaxLength)} must be positive (greater than 0).", true);
            return Vector3.Inf;
        }
        
        Camera3D? camera = GetViewport().GetCamera3D();

        if (camera is null) {
            Logger.Core.Error("There's no active camera in the current scene.", true);
            return Vector3.Inf;
        }

        Vector2 mousePosition = GetViewport().GetMousePosition();
        _rayQuery.From = camera.ProjectRayOrigin(mousePosition);
        _rayQuery.To = _rayQuery.From + camera.ProjectRayNormal(mousePosition) * pMaxLength;

        Dictionary collisionResult = GetWorld3D().DirectSpaceState.IntersectRay(_rayQuery);

        if (collisionResult.Count != 0) {
            return collisionResult["position"].AsVector3();
        }

        return _rayQuery.To;
    }

    public Entity CreateDummyEntity(Vector3 pPosition) {
        Entity? entity = AssetDB.DummyEntity.Instantiate<Entity>();
        entity.GlobalPosition = pPosition;
        AddChild(entity);
        
        // HACK: this should make the entity live for 2 frames.
        entity.CallDeferred(Node.MethodName.QueueFree);
        return entity;
    }
}