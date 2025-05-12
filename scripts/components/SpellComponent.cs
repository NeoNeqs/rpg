using Godot;
using RPG.scripts.effects;

namespace RPG.scripts.components;

public partial class SpellComponent : GizmoComponent {
    [Export] public required Effect[] Effects;
    [Export] public ulong CooldownSeconds;

    private ulong _lastCastTime = Time.GetTicksUsec();

    public void Cast() {
        _lastCastTime = Time.GetTicksUsec();
    }

    public bool IsOnCooldown() {
        ulong currentTime = Time.GetTicksUsec();

        ulong cooldownMicroseconds = CooldownSeconds * 1_000_000;
        return (_lastCastTime + cooldownMicroseconds > currentTime);
    }
}