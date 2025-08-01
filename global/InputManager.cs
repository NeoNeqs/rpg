using Godot;

namespace RPG.global;

public static class InputManager {
    public static bool IsTyping = false;

    public static bool IsActionJustPressed(string pAction) {
        return !IsTyping && Input.IsActionJustPressed(pAction);
    }

    public static Vector2 GetVector(string pNegX, string pPosX, string pNegY, string pPosY, float pDeadzone = -1.0f) {
        if (IsTyping) {
            return Vector2.Zero;
        }

        return Input.GetVector(pNegX, pPosX, pNegY, pPosY, pDeadzone);
    }
}