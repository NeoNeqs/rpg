using RPG.scripts.effects;
using RPG.scripts.inventory;

namespace RPG.ui.views.effect;

public partial class EffectSlot : Slot {

    public void Update((Gizmo, Effect) pData) {
        SetOnCooldown(pData.Item2.GetTimeLeft());
        IconHolder.Texture = pData.Item1.GetCurrentIcon();
    }

}