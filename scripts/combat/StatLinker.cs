using RPG.global;
using RPG.global.enums;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;

namespace RPG.scripts.combat;

public class StatLinker {
    public readonly Stats Total = new();

    public void Link(Stats? pStats) {
        if (pStats is null) {
            return;
        }

        if (!pStats.IsConnectedToIntegerStatChanged(OnIntegerStatChanged)) {
            foreach (IntegerStat stat in Stats.GetIntegerStats()) {
                long currentTotal = Total.GetIntegerStat(stat);
                Total.SetIntegerStat(stat, currentTotal + pStats.GetIntegerStat(stat));
            }

            pStats.IntegerStatChanged += OnIntegerStatChanged;
        }

        if (!pStats.IsConnectedToDecimalStatChanged(OnDecimalStatChanged)) {
            foreach (DecimalStat stat in Stats.GetDecimalStats()) {
                float currentTotal = Total.GetDecimalStat(stat);
                Total.SetDecimalStat(stat, currentTotal + pStats.GetDecimalStat(stat));
            }

            pStats.DecimalStatChanged += OnDecimalStatChanged;
        }
    }

    public void Unlink(Stats? pStats) {
        if (pStats is null) {
            return;
        }
        
        if (pStats.IsConnectedToIntegerStatChanged(OnIntegerStatChanged)) {
            foreach (IntegerStat stat in Stats.GetIntegerStats()) {
                long currentTotal = Total.GetIntegerStat(stat);
                Total.SetIntegerStat(stat, currentTotal - pStats.GetIntegerStat(stat));
            }

            pStats.IntegerStatChanged -= OnIntegerStatChanged;
        }

        if (pStats.IsConnectedToDecimalStatChanged(OnDecimalStatChanged)) {
            foreach (DecimalStat stat in Stats.GetDecimalStats()) {
                float currentTotal = Total.GetDecimalStat(stat);
                Total.SetDecimalStat(stat, currentTotal - pStats.GetDecimalStat(stat));
            }

            pStats.DecimalStatChanged -= OnDecimalStatChanged;
        }
    }

    public void LinkFromInventory(Inventory? pArmory) {
        if (pArmory is not null) {
            pArmory.GizmoChanged += LinkGizmoStatComponent;
            pArmory.GizmoAboutToChange += UnlinkGizmoStatComponent;
        }

        foreach (GizmoStack gizmoStack in pArmory?.Gizmos ?? []) {
            LinkGizmoStatComponent(gizmoStack, -1);
        }
    }

    private void LinkGizmoStatComponent(GizmoStack pGizmoStack, int pIndex) {
        var component = pGizmoStack.Gizmo?.GetComponent<StatComponent>();

        if (component is null) {
            return;
        }

        Link(component.Stats);
    }

    private void UnlinkGizmoStatComponent(GizmoStack pGizmoStack, int pIndex) {
        if (pGizmoStack.Gizmo is null) {
            return;
        }

        var component = pGizmoStack.Gizmo.GetComponent<StatComponent>();

        if (component is null) {
            Logger.Combat.Error(
                $"Can't unlink stats from {nameof(Gizmo)} Name={pGizmoStack.Gizmo.DisplayName} as it does not have a {nameof(StatComponent)}.");
            return;
        }

        Unlink(component.Stats);
    }

    private void OnIntegerStatChanged(IntegerStat pStat, long pDelta) {
        Total.SetIntegerStat(pStat, pDelta);
    }

    private void OnDecimalStatChanged(DecimalStat pStat, float pDelta) {
        Total.SetDecimalStat(pStat, pDelta);
    }
}