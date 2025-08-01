using System;
using global::RPG.global;
using Godot;
using Godot.Collections;
using RPG.global.singletons;
using RPG.scripts.effects;
using RPG.world.character;
using RPG.world.entity;

namespace RPG.world;

[GlobalClass]
public partial class World : Node3D {
    public Array<Dictionary> IntersectSphere(Vector3 pPosition, float pRadius, bool pBodyCollision, uint pCollisionMask,
        Array<Rid> pExclude) {
        Rid shapeRid = PhysicsServer3D.SphereShapeCreate();
        PhysicsServer3D.ShapeSetData(shapeRid, Variant.From(pRadius));

        Array<Dictionary> results = GetWorld3D().DirectSpaceState.IntersectShape(new PhysicsShapeQueryParameters3D {
            CollideWithBodies = pBodyCollision,
            CollisionMask = pCollisionMask,
            Transform = new Transform3D(Basis.Identity, pPosition),
            Exclude = pExclude,
            ShapeRid = shapeRid
        });

        PhysicsServer3D.FreeRid(shapeRid);

        return results;
    }

    public Dictionary IntersectRayAtMouse(float pMaxLength, uint pMask = 1) {
        Camera3D camera = GetViewport().GetCamera3D();
        Vector2 mousePosition = GetViewport().GetMousePosition();

        // Use last known (good) mouse position
        if (MouseStateMachine.IsCaptured()) {
            mousePosition = MouseStateMachine.Instance.LastMousePosition;
        }

        Vector3 cameraRayOrigin = camera.ProjectRayOrigin(mousePosition);
        Vector3 cameraRayDir = camera.ProjectRayNormal(mousePosition);
        Vector3 destination = cameraRayOrigin + cameraRayDir * pMaxLength;

        Dictionary result = GetWorld3D().DirectSpaceState.IntersectRay(new PhysicsRayQueryParameters3D {
            CollisionMask = pMask,
            CollideWithBodies = true,
            CollideWithAreas = false,
            From = cameraRayOrigin,
            To = destination
        });

        if (result.Count == 0) {
            result["position"] = destination;
        }

        return result;
    }

    // TODO: GetMouseWorldPositionWithLoS
    public Vector3 GetMouseWorldPosition(Vector3 pFrom, float pMaxRange, uint pCollisionMask = 1) {
        Camera3D camera = GetViewport().GetCamera3D();
        Vector2 mousePosition = GetViewport().GetMousePosition();

        // Use last known (good) mouse position
        if (MouseStateMachine.IsCaptured()) {
            mousePosition = MouseStateMachine.Instance.LastMousePosition;
        }

        Vector3 cameraRayOrigin = camera.ProjectRayOrigin(mousePosition);
        Vector3 cameraRayDir = camera.ProjectRayNormal(mousePosition);

        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;

        Dictionary rayHit = spaceState.IntersectRay(new PhysicsRayQueryParameters3D {
            From = cameraRayOrigin,
            To = cameraRayOrigin + cameraRayDir * 1_000_000,
            CollisionMask = pCollisionMask
        });

        Vector3 mouseWorldTarget;

        if (rayHit.Count > 0) {
            mouseWorldTarget = rayHit["position"].As<Vector3>();
        } else {
            // Nothing hit - use direction from the player to mouse ray and clamp it
            Vector3 dirFromPlayerToMouse = (cameraRayOrigin + cameraRayDir * 1000 - pFrom).Normalized();
            mouseWorldTarget = pFrom + dirFromPlayerToMouse * pMaxRange;
        }

        Vector3 offset = mouseWorldTarget - pFrom;
        float distanceSquared = offset.LengthSquared();

        if (distanceSquared > pMaxRange * pMaxRange) {
            offset = offset.Normalized();
            mouseWorldTarget = pFrom + offset * pMaxRange;
        }

        Vector3 groundRayOrigin = mouseWorldTarget + Vector3.Up * 1000;
        Vector3 groundRayEnd = mouseWorldTarget + Vector3.Down * 1000;

        Dictionary groundHit = spaceState.IntersectRay(new PhysicsRayQueryParameters3D {
            From = groundRayOrigin,
            To = groundRayEnd,
            CollisionMask = pCollisionMask
        });

        if (groundHit.Count > 0) {
            return groundHit["position"].As<Vector3>();
        }

        return mouseWorldTarget;
    }

    public Vector3 GetMouseWorldPosition(float pMaxLength) {
        Dictionary collisionResult = IntersectRayAtMouse(pMaxLength);
        return collisionResult["position"].AsVector3();
    }

    public void CreateAoe(Vector3 pPosition, float pRadius, Effect pEffect, Action<Entity> pEnteredCallback,
        Action<Entity> pExitedCallback) {
        var detectionArea = new Area3D {
            Monitoring = true,
            CollisionLayer = 3, // TODO: 0 ???
            CollisionMask = 3
        };

        var shape = new CylinderShape3D {
            Radius = pRadius,
            Height = 100
        };

        var collisionShape = new CollisionShape3D {
            Shape = shape
        };
        collisionShape.DebugColor = Color.FromHtml("#ffffffff");
        collisionShape.DebugFill = true;
        detectionArea.Connect(Area3D.SignalName.BodyEntered, Callable.From((Action<Node3D>)BodyEntered));
        detectionArea.Connect(Area3D.SignalName.BodyExited, Callable.From((Action<Node3D>)BodyExited));

        pEffect.Connect(
            Effect.SignalName.Finished,
            Callable.From(() => {
                foreach (Node3D overlappingBody in detectionArea.GetOverlappingBodies()) {
                    BodyExited(overlappingBody);
                }

                detectionArea.Disconnect(Area3D.SignalName.BodyEntered, Callable.From((Action<Node3D>)BodyEntered));
                detectionArea.Disconnect(Area3D.SignalName.BodyExited, Callable.From((Action<Node3D>)BodyExited));
                detectionArea.QueueFree();
            }),
            (uint)ConnectFlags.OneShot | (uint)ConnectFlags.ReferenceCounted
        );


        detectionArea.AddChild(collisionShape);
        AddChild(detectionArea);
        detectionArea.Visible = true;
        detectionArea.GlobalPosition = pPosition;

        return;

        void BodyEntered(Node3D pBody) {
            if (pBody.GetParent() is Entity ent) {
                pEnteredCallback.Invoke(ent);
            }
        }

        void BodyExited(Node3D pBody) {
            if (pBody.GetParent() is Entity ent) {
                pExitedCallback.Invoke(ent);
            }
        }
    }

    // public List<Entity> GetEntitiesInRadius(Vector3 pPosition, float pRadius, Array<Rid> pExclude) {
    //     var result = new List<Entity>();
    //
    //     Array<Dictionary> queryResults = IntersectSphere(
    //         pPosition,
    //         pRadius,
    //         true,
    //         2,
    //         pExclude
    //     );
    //
    //     foreach (Dictionary queryResult in queryResults) {
    //         GodotObject collider = queryResult["collider"].AsGodotObject();
    //
    //         // TODO: Add LoS checks. After we have all Entities in radius check if a raycast can hit that Entity. 
    //         //       Only then add it.
    //
    //         if (collider is PhysicsBody3D body && body.GetParent() is Entity ent) {
    //             result.Add(ent);
    //         }
    //     }
    //
    //     return result;
    // }


    public Entity CreateTempDummyEntity(Vector3 pPosition) {
        var entity = AssetDB.DummyEntity.Instantiate<DummyEntity>();
        entity.LifetimeInFrames = 1;
        AddChild(entity);
        entity.GlobalPosition = pPosition;

        return entity;
    }

    public PlayerCharacter? GetPlayer() {
        return GetNodeOrNull<PlayerCharacter>("Character");
    }

    public AoeDecal GetDecal() {
        return GetNode<AoeDecal>("AoeDecal");
    }
}