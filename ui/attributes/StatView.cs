using Godot;
using RPG.scripts.combat;

namespace RPG.ui.attributes;

[Tool, GlobalClass]
public partial class StatView : View {
    [Export] private RichTextLabel _label = null!;

    [Export]
    private string Title {
        get {
            if (!IsNodeReady()) {
                return "";
            }

            return GetNode<Label>("VBoxContainer/Label").Text;
        }
        set {
            if (!IsNodeReady()) {
                return;
            }

            GetNode<Label>("VBoxContainer/Label").Text = value;
        }
    }

    public void SetData(Stats pStats, string pTitle) {
        _label.Clear();

        foreach (Stats.IntegerStat integerStat in Stats.GetIntegerStats()) {
            _label.AppendText($"{integerStat.ToString()}: {pStats.GetIntegerStat(integerStat)}");
        }

        foreach (Stats.DecimalStat decimalStat in Stats.GetDecimalStats()) {
            _label.AppendText($"{decimalStat.ToString()}: {pStats.GetDecimalStat(decimalStat)}");
        }
        
        Title = pTitle;
    }
}