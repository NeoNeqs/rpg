using Godot;
using RPG.global.enums;

namespace RPG.scripts.inventory.components;

[Tool, GlobalClass]
public partial class ItemComponent : GizmoComponent {
    [Export] public ItemRarity ItemRarity = ItemRarity.Common;
    [Export] public long Level = 1;

    public override bool IsAllowed(GizmoComponent pGizmoComponent) {
        return pGizmoComponent is ItemComponent;
    }

    public Color GetRarityColor() {
        return ItemRarity switch {
            ItemRarity.Common => Colors.Gray,
            ItemRarity.Uncommon => Colors.LimeGreen,
            ItemRarity.Rare => Colors.DodgerBlue,
            ItemRarity.Epic => Colors.BlueViolet,
            ItemRarity.Legendary => Colors.OrangeRed,
            _ => Colors.Black
        };
    }
}