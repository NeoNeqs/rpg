using global::RPG.global;
using Godot;
using RPG.scripts.combat;

namespace RPG.ui.views.effect;

public partial class EffectView : View<AppliedEffect> {
    public void InitializeWith(SpellManager? pContainer) {
        if (Container == pContainer) {
            return;
        }

        // Disconnect old signals
        if (Container is SpellManager combatManager) {
            combatManager.AppliedEffect -= OnEffectApplied;
            combatManager.RemovedEffect -= OnEffectRemoved;

            foreach (Node node in SlotHolder.GetChildren()) {
                SlotHolder.RemoveChild(node);
                node.QueueFree();
            }
        }

        Container = pContainer;
        if (Container is not null) {
            combatManager = GetCombatManager();
            combatManager.AppliedEffect += OnEffectApplied;
            combatManager.RemovedEffect += OnEffectRemoved;
            ResizeHolder();
        }
    }

    private void OnEffectRemoved(AppliedEffect pAppliedEffect, int pIndex) {
        var slot = GetSlot<EffectSlot>(pIndex)!;
        SlotHolder.RemoveChild(slot);
        slot.QueueFree();
    }

    // TODO: make this take ValueTuple as a parameter
    private void OnEffectApplied(AppliedEffect pAppliedEffect) {
        ResizeHolder();

        var slot = GetSlot<EffectSlot>(SlotHolder.GetChildCount() - 1)!;
        slot.Update(pAppliedEffect);
    }

    protected override void AddSlot(int pIndex) {
        if (Container is null) {
            Logger.UI.Critical("BUG! Container should not be null here!", true);
            return;
        }

        AppliedEffect data = Container.GetAt(pIndex);
        var slot = SlotScene.Instantiate<EffectSlot>();
        slot.Update(data);

        slot.LeftMouseButtonPressed += () => EmitSignalSlotPressed(this, slot, false);
        slot.RightMouseButtonPressed += () => EmitSignalSlotPressed(this, slot, true);
        slot.Hovered += () => EmitSignalSlotHovered(this, slot);
        slot.Unhovered += EmitSignalSlotUnhovered;
        SlotHolder.AddChildOwned(slot, this);
    }

    public SpellManager GetCombatManager() {
        return (SpellManager)Container!;
    }
}