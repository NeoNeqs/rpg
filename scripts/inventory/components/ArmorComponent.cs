using Godot;
using RPG.global.enums;

namespace RPG.scripts.inventory.components;

[Tool, GlobalClass]
public partial class ArmorComponent : GizmoComponent {
    private long _currentDurability;
    [Export] public ArmorSlot ArmorSlot;
    [Export] public ArmorType ArmorType;
    [Export] public long MaxDurability;

    public override bool IsAllowed(GizmoComponent pOtherComponent) {
        return pOtherComponent is ArmorComponent armorComponent &&
               ArmorSlot == armorComponent.ArmorSlot &&
               ArmorType <= armorComponent.ArmorType;
    }
}