using Godot;
using RPG.global.singletons;

namespace RPG.ui;

public partial class Frames : Control {
    public override void _EnterTree() {
        EventBus.Instance.PlayerTargetChanged += (pCharacter, pTarget) => {
            GetNodeOrNull<EntityFrame>("TargetFrame").Update(pTarget?.CombatManager);
        };
    }
}