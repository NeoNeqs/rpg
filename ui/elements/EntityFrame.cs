using RPG.scripts.combat;

namespace RPG.ui.elements;

public partial class EntityFrame : elements.UIElement {
    public void UpdateEffectView(CombatManager? pCombatManager) {
        GetNodeOrNull<views.effect.EffectView>("VBoxContainer/EffectView").InitializeWith(pCombatManager);
    }
}