using Godot;

namespace RPG.scripts.inventory.components;

public partial class ItemComponent : GizmoComponent {
    public enum Rarity {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
    }

    [Export] public Rarity ItemRarity = Rarity.Common;
    [Export] public long Level = 1;


    public override bool IsAllowed(GizmoComponent pGizmoComponent) {
        return pGizmoComponent is ItemComponent;
    }
}