using Godot;

namespace RPG.world.entity;

public partial class DummyEntity : Entity {
    [Export] public long LifetimeInFrames = -1;

    // private long _aliveFrameCount = 0;
    //
    // public override void _Ready() {
    //     SetProcess(LifetimeInFrames != -1);
    // }
    //
    // public override void _Process(double pDelta) {
    //     _aliveFrameCount++;
    //
    //     if (_aliveFrameCount >= LifetimeInFrames) {
    //         QueueFree();
    //     }
    // }
}