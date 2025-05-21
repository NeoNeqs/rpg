using Godot;
using RPG.scripts.effects;

namespace RPG.scripts.inventory.components;

[Tool, GlobalClass]
public partial class SpellComponent : GizmoComponent {
    [Export] public required Effect[] Effects;
    [Export] public ulong CooldownSeconds;

    private ulong _lastCastTime = Time.GetTicksUsec();

    public virtual bool Cast() {
        _lastCastTime = Time.GetTicksUsec();
        return true;
    }

    public bool IsOnCooldown() {
        ulong currentTime = Time.GetTicksUsec();

        ulong cooldownMicroseconds = CooldownSeconds * 1_000_000;
        return (_lastCastTime + cooldownMicroseconds > currentTime);
    }

    public ulong GetRemainingCooldown() {
        ulong cooldownMicroseconds = CooldownSeconds * 1_000_000;
        return (_lastCastTime + cooldownMicroseconds - Time.GetTicksUsec());
    }
}