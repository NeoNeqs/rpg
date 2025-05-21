using System.Text.RegularExpressions;
using Godot;

namespace RPG.ui;

[GlobalClass]
public partial class PoorTextLabel : VBoxContainer {
    private static readonly Regex TagRegex = MyRegex();

    [GeneratedRegex("([^#]+|#[^#]*#)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex MyRegex();


    public void Update(string pText) {
        Clear();

        string[] lines = pText.Split('\n');

        foreach (string line in lines) {
            ParseLine(line);
        }
    }

    private void ParseLine(string pLine) {
        Label currentLabel = CreateLabel(this);

        foreach (Match match in TagRegex.Matches(pLine)) {
            // foreach (Group matchGroup in match.Groups) {
            if (match.Value == "#right#") {
                currentLabel = HandleRightTag(currentLabel);
            } else if (match.Value.StartsWith("#color=")) {
                currentLabel = HandleColorTag(currentLabel, match.Value);
            } else if (match.Value.StartsWith("#size=")) {
                currentLabel = HandleSizeTag(currentLabel, match.Value);
            } else {
                currentLabel.Text += match.Value;
            }
            // }
        }
    }


    private HBoxContainer GetHbox(Label pRelative) {
        Node? node = pRelative.GetParent();

        if (node is not HBoxContainer hbox) {
            hbox = new HBoxContainer();
            hbox.AddThemeConstantOverride("separation", 0);
            AddChild(hbox);
            pRelative.CustomMinimumSize = new Vector2(200, 0);
            pRelative.Reparent(hbox);
            pRelative.AutowrapMode = TextServer.AutowrapMode.Off;
        }

        return hbox;
    }


    private Label HandleRightTag(Label pCurrentLabel) {
        if (pCurrentLabel.Text.Length == 0) {
            pCurrentLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            pCurrentLabel.HorizontalAlignment = HorizontalAlignment.Right;
            return pCurrentLabel;
        }

        HBoxContainer hbox = GetHbox(pCurrentLabel);
        pCurrentLabel.SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
        Label newLabel = CreateLabel(hbox);
        newLabel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        newLabel.HorizontalAlignment = HorizontalAlignment.Right;

        return newLabel;
    }

    private Label HandleColorTag(Label pCurrentLabel, string pTag) {
        string colorCode = pTag.Substring(7, pTag.Length - 8);
        
        Color color = Color.FromString(colorCode, Colors.White);

        if (pCurrentLabel.Text.Length == 0) {
            pCurrentLabel.LabelSettings.FontColor = color;
            return pCurrentLabel;
        }

        HBoxContainer hbox = GetHbox(pCurrentLabel);
        Label newLabel = CreateLabel(hbox);
        newLabel.LabelSettings.FontColor = color;
        return newLabel;
    }

    private Label HandleSizeTag(Label pCurrentLabel, string pTag) {
        string sizeStr = pTag.Substring(6, pTag.Length - 7);
        int sizeNum = sizeStr.ToInt();

        if (pCurrentLabel.Text.Length == 0) {
            pCurrentLabel.LabelSettings.FontSize = sizeNum;
            return pCurrentLabel;
        }

        HBoxContainer hbox = GetHbox(pCurrentLabel);
        Label newLabel = CreateLabel(hbox);
        newLabel.LabelSettings.FontSize = sizeNum;

        return newLabel;
    }

    public void Clear() {
        foreach (Node child in GetChildren()) {
            child.QueueFree();
            RemoveChild(child);
        }
    }


    private static Label CreateLabel(Node pParent) {
        Label label = new() {
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
            HorizontalAlignment = HorizontalAlignment.Left,
            LabelSettings = new LabelSettings() {
                FontColor = Colors.White
            }
        };

        if (pParent is HBoxContainer) {
            foreach (Node child in pParent.GetChildren()) {
                if (child is not Control childControl) {
                    continue;
                }

                childControl.CustomMinimumSize = Vector2.Zero;
            }
        } else {
            label.CustomMinimumSize = new Vector2(400, 0);
            label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        }

        pParent.AddChild(label);
        return label;
    }
}