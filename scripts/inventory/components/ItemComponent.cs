using Godot;

namespace RPG.scripts.inventory.components;

[Tool, GlobalClass]
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

    public Color GetRarityColor() {
        return ItemRarity switch {
            Rarity.Common => Colors.Gray,
            Rarity.Uncommon => Colors.LimeGreen,
            Rarity.Rare => Colors.DodgerBlue,
            Rarity.Epic => Colors.BlueViolet,
            Rarity.Legendary => Colors.OrangeRed,
            _ => Colors.Black,
        };
    }
}