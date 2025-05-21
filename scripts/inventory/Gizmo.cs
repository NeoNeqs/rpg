using System;
using Godot;
using Godot.Collections;
using RPG.Global;
using RPG.scripts.inventory.components;

namespace RPG.scripts.inventory;

[GlobalClass]
public partial class Gizmo : ComponentSystem<GizmoComponent> {
    [Signal]
    public delegate void CastedEventHandler(ulong pCooldownInMicroSeconds);

    [Export] public required string DisplayName;
    [Export] public int StackSize;

    public void EmitCasted(ulong pCooldownInMicroSeconds) {
        EmitSignalCasted(pCooldownInMicroSeconds);
    }

    public ulong GetCooldown() {
        var chainSpellComponent = GetComponent<ChainSpellComponent>();

        if (chainSpellComponent != null) {
            return chainSpellComponent.CooldownSeconds;
        }

        var spellComponent = GetComponent<SpellComponent>();

        if (spellComponent != null) {
            return spellComponent.CooldownSeconds;
        }

        return 0;
    }

#if DEBUG
    public override bool _Set(StringName pRoperty, Variant pValue) {
        if (pRoperty == _componentsExportName) {
            int counter = 0;
            var components = pValue.As<Array<GizmoComponent>>();
            foreach (GizmoComponent component in components) {
                if (component is ChainSpellComponent or SpellComponent) {
                    counter++;
                }
            }

            if (counter >= 2) {
                Logger.Core.Warn(
                    $"There should be at most 1 {nameof(ChainSpellComponent)} or 1 {nameof(SpellComponent)}");
            }
        }

        return base._Set(pRoperty, pValue);
    }
#endif
}