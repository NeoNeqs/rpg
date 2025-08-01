using Godot;

namespace RPG.global;

public static class RNG {
    /// <summary>
    ///     Rolls a die. Any rolled number below or equal <paramref name="pChance" /> is considered as a successful roll and
    ///     above as a failed roll.
    /// </summary>
    /// <param name="pChance">Probability (0 to 1) of a successful roll.</param>
    /// <returns><c>True</c> if the roll was successful, otherwise <c>False</c>.</returns>
    public static bool Roll(float pChance) {
        switch (pChance) {
            case >= 1.0f:
                return true;
            case <= 0.0f:
                return false;
            default: {
                float chance = GD.Randf();
                return chance <= pChance;
            }
        }
    }
}