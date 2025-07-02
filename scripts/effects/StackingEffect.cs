using Godot;
using RPG.scripts.combat;

namespace RPG.scripts.effects;

public abstract partial class StackingEffect : Effect {
    [Export(PropertyHint.Range, "0, 2147483647, 1")]
    public uint FlatValue { private set; get; }

    [Export] public Stats.IntegerStat StatScale { private set; get; }

    [Export(PropertyHint.Range, "0, 255, 0.1")]
    public float StatScaleCoefficient { private set; get; }

    [Export(PropertyHint.Range, "1, 65535, 1")]
    public ushort MaxStacks { private set; get; } = 1;

    // Always start with minimum stacks!
    public int CurrentStacks { private set; get; } = 1;
    
    
    public void Stack() {
        if (CurrentStacks < MaxStacks) {
            CurrentStacks++;
        }

        // Reset the timer since new stack was just applied
        if (Timer?.IsInsideTree() ?? false) {
            Timer.Start();
        }
    }
}