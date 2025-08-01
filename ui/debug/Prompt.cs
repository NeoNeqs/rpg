using global::RPG.global;
using Godot;

namespace RPG.ui.debug;

public partial class Prompt : LineEdit {
    public override void _Ready() {
        FocusEntered += () => { InputManager.IsTyping = true; };
        FocusExited += () => { InputManager.IsTyping = false; };
    }
}