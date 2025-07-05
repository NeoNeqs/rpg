#if TOOLS
using System.Diagnostics;
using Godot;
using RPG.scripts;
using RPG.scripts.combat;
using RPG.scripts.effects;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;

namespace RPG.global.tools;

public sealed class Tools {
    [Conditional("TOOLS")]
    public static void Assert(bool pCondition, string pMessage = "") {
        if (!pCondition) {
            GD.PushError(pMessage);
        }
    }
}

// A neat trick to do compile time asserts, although the thrown error message is not very useful.
// Source: https://www.lunesu.com/archives/62-Static-assert-in-C!.html
// ReSharper disable once UnusedType.Local
internal sealed class StaticAssert {
    // IMPORTANT: Changing those names will break existing resources!
#pragma warning disable CS0414 // Field is assigned but its value is never used
    // ReSharper disable HeuristicUnreachableCode
    private byte _assert1 = nameof(ChainSpellComponent.Spells) == "Spells" ? 0 : -1;

    private byte _assert2 = nameof(ArmorComponent.ArmorSlot) == "ArmorSlot" ? 0 : -1;
    private byte _assert3 = nameof(ArmorComponent.ArmorType) == "ArmorType" ? 0 : -1;
    private byte _assert4 = nameof(ArmorComponent.MaxDurability) == "MaxDurability" ? 0 : -1;

    private byte _assert5 = nameof(DamageComponent.DamageType) == "DamageType" ? 0 : -1;
    private byte _assert6 = nameof(DamageComponent.Damage) == "Damage" ? 0 : -1;

    private byte _assert7 = nameof(ItemComponent.ItemRarity) == "ItemRarity" ? 0 : -1;
    private byte _assert8 = nameof(ItemComponent.Level) == "Level" ? 0 : -1;

    private byte _assert9 = nameof(SpellComponent.Effects) == "Effects" ? 0 : -1;
    private byte _assert10 = nameof(SpellComponent.CooldownSeconds) == "CooldownSeconds" ? 0 : -1;
    private byte _assert11 = nameof(SpellComponent.Range) == "Range" ? 0 : -1;
    private byte _assert12 = nameof(StatComponent.Stats) == "Stats" ? 0 : -1;

    private byte _assert13 = nameof(Effect.Id) == "Id" ? 0 : -1;
    private byte _assert14 = nameof(Effect.DisplayName) == "DisplayName" ? 0 : -1;
    private byte _assert15 = nameof(Effect.Icon) == "Icon" ? 0 : -1;
    private byte _assert16 = nameof(Effect.TickTimeout) == "TickTimeout" ? 0 : -1;
    private byte _assert17 = nameof(Effect.TotalTicks) == "TotalTicks" ? 0 : -1;
    private byte _assert18 = nameof(Effect.ApplicationChance) == "ApplicationChance" ? 0 : -1;
    private byte _assert19 = nameof(Effect.Radius) == "Radius" ? 0 : -1;
    private byte _assert20 = nameof(Effect.Flags) == "Flags" ? 0 : -1;
    
    private byte _assert21 = nameof(DamageEffect.DamageType) == "DamageType" ? 0 : -1;
    
    private byte _assert22 = nameof(StackingEffect.FlatValue) == "FlatValue" ? 0 : -1;
    private byte _assert23 = nameof(StackingEffect.StatScale) == "StatScale" ? 0 : -1;
    private byte _assert24 = nameof(StackingEffect.StatScaleCoefficient) == "StatScaleCoefficient" ? 0 : -1;
    private byte _assert25 = nameof(StackingEffect.MaxStacks) == "MaxStacks" ? 0 : -1;

    private byte _assert26 = nameof(StatEffect.Stat) == "Stat" ? 0 : -1;
    
    private byte _assert27 = nameof(StatusEffect.CrowdControlImmunity) == "CrowdControlImmunity" ? 0 : -1;
    private byte _assert28 = nameof(StatusEffect.CrowdControl) == "CrowdControl" ? 0 : -1;
    
    private byte _assert29 = nameof(Gizmo.Id) == "Id" ? 0 : -1;
    private byte _assert30 = nameof(Gizmo.DisplayName) == "DisplayName" ? 0 : -1;
    private byte _assert31 = nameof(Gizmo.Icon) == "Icon" ? 0 : -1;
    private byte _assert32 = nameof(Gizmo.StackSize) == "StackSize" ? 0 : -1;
    
    private byte _assert33 = nameof(GizmoStack.Gizmo) == "Gizmo" ? 0 : -1;
    private byte _assert34 = nameof(GizmoStack.Quantity) == "Quantity" ? 0 : -1;
    private byte _assert35 = nameof(GizmoStack.AllowedComponents) == "AllowedComponents" ? 0 : -1;
    
    private byte _assert36 = nameof(Inventory.Gizmos) == "Gizmos" ? 0 : -1;
    private byte _assert37 = nameof(Inventory.Columns) == "Columns" ? 0 : -1;
    private byte _assert38 = nameof(Inventory.AllowedComponents) == "AllowedComponents" ? 0 : -1;
    private byte _assert39 = nameof(Inventory.Flags) == "Flags" ? 0 : -1;
    
    private byte _assert40 = nameof(ComponentSystem<Resource>.Components) == "Components" ? 0 : -1;
    private byte _assert41 = ComponentSystem<Resource>.ComponentsExportName == "components" ? 0 : -1;
    
    // ReSharper restore HeuristicUnreachableCode
#pragma warning restore CS0414 // Field is assigned but its value is never used
}

#endif