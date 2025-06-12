using Godot;

namespace RPG.global;

// ReSharper disable once InconsistentNaming
public static class RNG {

    public static bool Roll(double pRoll) {
        if (pRoll >= 1) {
            return true;
        }
        
        float chance = GD.Randf();
        return chance > pRoll;

    }
}