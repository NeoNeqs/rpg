using Godot;
using RPG.global.singletons;

namespace RPG.world.entity;

[GlobalClass]
public partial class Enemy : Entity {
    public override void _EnterTree() {
        base._EnterTree();
        GetBody().BodySelected += () => { EventBus.Instance.EmitEntitySelectedEventHandler(this); };
    }

    public RigidBody GetBody() {
        return GetChild<RigidBody>(0);
    }
}