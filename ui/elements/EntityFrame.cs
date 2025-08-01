using RPG.scripts.combat;
using RPG.ui.views.effect;

namespace RPG.ui.elements;

public partial class EntityFrame : UIElement {
    public void UpdateEffectView(SpellManager? pCombatManager) {
        GetNodeOrNull<EffectView>("VBoxContainer/EffectView").InitializeWith(pCombatManager);
    }
}