using Godot;
using RPG.global.tools;
using RPG.ui;
using RPG.world;

namespace RPG;

[GlobalClass]
public partial class Main : Node {
    public override void _Ready() {
        Tools.Assert(GetChild<UI>(0) is not null, $"{nameof(UI)} node must be the first child in the scene.");
        Tools.Assert(GetChild<World>(1) is not null, $"{nameof(World)} node must be the first child in the scene.");
        Tools.Assert(GetChildCount() == 2, $"{nameof(Main)} node must not contain more than 2 nodes.");
    }

    public UI GetUI() {
        return GetChild<UI>(0);
    }

    public World GetWorld() {
        return GetChild<World>(1);
    }
}