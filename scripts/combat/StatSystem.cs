using Godot;

namespace RPG.scripts.combat;

[GlobalClass]
public partial class StatSystem : Resource {
    public Stats Total = new();

    public void Link(Stats pStats) {
        if (pStats.IsConnectedToIntegerStatChanged(OnIntegerStatChanged)) {
            return;
        }

        foreach (Stats.IntegerStat stat in Stats.GetIntegerStats()) {
            long currentTotal = Total.GetIntegerStat(stat);
            Total.SetIntegerStat(stat, currentTotal + pStats.GetIntegerStat(stat));
        }

        pStats.IntegerStatChanged += OnIntegerStatChanged;

        if (pStats.IsConnectedToDecimalStatChanged(OnDecimalStatChanged)) {
            return;
        }

        foreach (Stats.DecimalStat stat in Stats.GetDecimalStats()) {
            float currentTotal = Total.GetDecimalStat(stat);
            Total.SetDecimalStat(stat, currentTotal + pStats.GetDecimalStat(stat));
        }

        pStats.DecimalStatChanged += OnDecimalStatChanged;
    }

    public void Unlink(Stats pStats) {
        if (pStats.IsConnectedToIntegerStatChanged(OnIntegerStatChanged)) {
            foreach (Stats.IntegerStat stat in Stats.GetIntegerStats()) {
                long currentTotal = Total.GetIntegerStat(stat);
                Total.SetIntegerStat(stat, currentTotal - pStats.GetIntegerStat(stat));
            }

            pStats.IntegerStatChanged -= OnIntegerStatChanged;
        }

        if (pStats.IsConnectedToDecimalStatChanged(OnDecimalStatChanged)) {
            foreach (Stats.DecimalStat stat in Stats.GetDecimalStats()) {
                float currentTotal = Total.GetDecimalStat(stat);
                Total.SetDecimalStat(stat, currentTotal - pStats.GetDecimalStat(stat));
            }

            pStats.DecimalStatChanged -= OnDecimalStatChanged;
        }
    }

    private void OnIntegerStatChanged(Stats.IntegerStat pStat, long pDelta) {
        Total.SetIntegerStat(pStat, pDelta);
    }

    private void OnDecimalStatChanged(Stats.DecimalStat pStat, float pDelta) {
        Total.SetDecimalStat(pStat, pDelta);
    }
}