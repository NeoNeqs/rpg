using Godot;
using RPG.global;

namespace RPG.ui.debug;

public partial class Prompt : LineEdit {
    public override void _Ready() {
        FocusEntered += () => { InputManager.IsTyping = true; };
        FocusExited += () => { InputManager.IsTyping = false; };
    }
}