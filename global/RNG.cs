using Godot;

namespace RPG.global;

public static class RNG {
    /// <summary>
    /// Rolls a die. Any rolled number below or equal <paramref name="pChance" /> is considered as a successful roll and above as a failed roll.
    /// </summary>
    /// <param name="pChance">Probability of a successful role.</param>
    /// <returns><c>True</c> if the roll was successful, otherwise <c>False</c>.</returns>
    public static bool Roll(double pChance) {
        switch (pChance) {
            case >= 1.0d:
                return true;
            case <= 0.0d:
                return false;
            default: {
                float chance = GD.Randf();
                return chance <= pChance;
            }
        }
    }
}