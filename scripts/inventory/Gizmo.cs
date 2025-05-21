using Godot;
using Godot.Collections;
using RPG.global;
using RPG.scripts.inventory.components;

namespace RPG.scripts.inventory;

[Tool, GlobalClass]
public partial class Gizmo : ComponentSystem<GizmoComponent> {
    [Signal]
    public delegate void CastedEventHandler(ulong pCooldownInMicroSeconds);

    [Export] public string DisplayName = null!;
    [Export] public Texture2D Icon = null!;
    [Export] public int StackSize = 1;

    public void EmitCasted(ulong pCooldownInMicroSeconds) {
        EmitSignalCasted(pCooldownInMicroSeconds);
    }

#if DEBUG
    public override bool _Set(StringName pRoperty, Variant pValue) {
        if (pRoperty == ComponentsExportName) {
            int counter = 0;
            var components = pValue.As<Array<GizmoComponent>>();
            foreach (GizmoComponent component in components) {
                if (component is ChainSpellComponent or SpellComponent) {
                    counter++;
                }
            }

            if (counter >= 2) {
                Logger.Core.Warn($"Gizmo '{ResourcePath}' cannot have both {nameof(ChainSpellComponent)} and {nameof(SpellComponent)}.");
            }
        }

        return base._Set(pRoperty, pValue);
    }
#endif
}