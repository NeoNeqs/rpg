using Godot;
using RPG.scripts.combat;

namespace RPG.scripts.effects;

[Tool, GlobalClass]
public sealed partial class StatEffect : StackingEffect {
    [Export] public Stats.IntegerStat Stat { private set; get; }
}