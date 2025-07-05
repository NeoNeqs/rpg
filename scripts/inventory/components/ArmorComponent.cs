using Godot;
using RPG.global.enums;

namespace RPG.scripts.inventory.components;

[Tool, GlobalClass]
public partial class ArmorComponent : GizmoComponent {
    [Export] public ArmorSlot ArmorSlot;
    [Export] public ArmorType ArmorType;
    [Export] public long MaxDurability;

    private long _currentDurability;

    public override bool IsAllowed(GizmoComponent pOtherComponent) {
        return (
            pOtherComponent is ArmorComponent armorComponent &&
            ArmorSlot == armorComponent.ArmorSlot &&
            ArmorType <= armorComponent.ArmorType
        );
    }
}