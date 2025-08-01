using RPG.scripts.combat;

namespace RPG.ui.views.effect;

public partial class EffectSlot : Slot {
    public void Update(AppliedEffect pData) {
        SetOnCooldown(pData.Effect.GetTimeLeft());
        IconHolder.Texture = pData.Effect.Icon;
    }
}