using Godot;
using RPG.world;
using EventBus = RPG.global.singletons.EventBus;

namespace RPG;

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