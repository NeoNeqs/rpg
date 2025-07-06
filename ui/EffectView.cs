using RPG.global;
using RPG.scripts.effects;
using RPG.scripts.inventory;

namespace RPG.ui;

public partial class EffectView : View<(Gizmo, Effect)> {
    protected override void AddSlot(int pIndex) {
        if (Container is null) {
            Logger.UI.Critical("BUG! Container should not be null here!", true);
            return;
        }
    }
}