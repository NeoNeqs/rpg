using Godot;

namespace RPG.scripts.inventory.components;

[Tool, GlobalClass]
public partial class ArmorComponent : GizmoComponent {
    public enum Slot {
        Head,
        Neck,
        Chest,
    }

    public enum Type {
        Light,
        Medium,
        Heavy,
    }

    [Export] public Slot ArmorSlot;
    [Export] public Type ArmorType;
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