using RPG.global;
using Godot;
using Godot.Collections;
using MouseStateMachine = RPG.global.singletons.MouseStateMachine;

namespace RPG.world;

[GlobalClass]
public partial class World : Node3D {
    private readonly PhysicsRayQueryParameters3D _rayQuery = new();
    private readonly PhysicsShapeQueryParameters3D _shapeQuery = new();

    [Export] private Decal _decal;

    public override void _EnterTree() {
        _rayQuery.CollideWithBodies = true;
        _rayQuery.CollisionMask = 1;

        _shapeQuery.ShapeRid = PhysicsServer3D.SphereShapeCreate();
    }

    public override void _ExitTree() {
        PhysicsServer3D.FreeRid(_shapeQuery.ShapeRid);
    }

    public Array<Dictionary> IntersectShape(Vector3 pPosition, float pRadius, bool pBodyCollision, uint pCollisionMask,
        Rid pExclude) {
        _shapeQuery.CollideWithBodies = pBodyCollision;
        _shapeQuery.CollisionMask = pCollisionMask;
        _shapeQuery.Transform = new Transform3D(Basis.Identity, pPosition);
        _shapeQuery.Exclude = [pExclude];

        PhysicsServer3D.ShapeSetData(_shapeQuery.ShapeRid, pRadius);

        return GetWorld3D().DirectSpaceState.IntersectShape(_shapeQuery);
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

        Vector2 mousePosition;

        // Use last known (good) position of the mouse, since the mouse jumps to the center of the screen during CameraControl state.
        if (MouseStateMachine.Instance.CurrentState == MouseStateMachine.State.CameraControl) {
            mousePosition = MouseStateMachine.Instance.LastMousePosition;
        } else {
            mousePosition = GetViewport().GetMousePosition();
        }

        _rayQuery.From = camera.ProjectRayOrigin(mousePosition);
        _rayQuery.To = _rayQuery.From + camera.ProjectRayNormal(mousePosition) * pMaxLength;

        Dictionary collisionResult = GetWorld3D().DirectSpaceState.IntersectRay(_rayQuery);

        if (collisionResult.Count != 0) {
            return collisionResult["position"].AsVector3();
        }

        return _rayQuery.To;
        // return Vector3.Inf;
    }

    public Entity CreateTempDummyEntity(Vector3 pPosition) {
        Entity? entity = AssetDB.DummyEntity.Instantiate<Entity>();
        AddChild(entity);
        entity.GlobalPosition = pPosition;

        // HACK: this should make the entity live for 2 frames.
        entity.CallDeferred(Node.MethodName.QueueFree);
        return entity;
    }
}