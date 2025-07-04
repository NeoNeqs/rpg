using Godot;
using RPG.global.enums;

namespace RPG.scripts.effects;

public partial class StatusEffect : Node {
    [Export] public CrowdControl CrowdControlImmunity;
    [Export] public CrowdControl CrowdControl;
}