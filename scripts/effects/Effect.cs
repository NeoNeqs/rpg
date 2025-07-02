using System;
using Godot;
using RPG.global;

namespace RPG.scripts.effects;

// TODO: Effect and Gizmo DB that will validate that every Effect and Gizmo has non-empty AND unique ID
// TODO: I need to be able to apply an effect for 1 tick, but remove it after TickTimeout seconds! so the IsImmediate needs to come back.
// TODO: Create test spells:
//       - 20 second buff that modifies a stat Immediately and clears it only after 20 seconds have passed
//       - Deadly Poison - stacking poison up to 5 times that deals 30 damage every 3 seconds
[Tool, GlobalClass]
public partial class Effect : Resource {
    [Signal]
    public delegate void TickEventHandler();

    [Signal]
    public delegate void FinishedEventHandler();

    [Flags]
    public enum EffectFlags : ulong {
        TargetAllies = 1 << 0,
        TargetSelfOnly = 1 << 1,
    }

    [Export]
    public StringName Id {
        private set {
#if TOOLS
            if (value.IsEmpty) {
                _id = DisplayName.ToSnakeCase();
                return;
            }
#endif
            _id = value;
        }
        get => _id;
    }

    [Export] public string DisplayName { private set; get; } = "";
    [Export] public Texture2D? Icon { private set; get; }

    // Note: TickTimeout refers to the time in seconds it takes to 1 application of the effect
    [Export(PropertyHint.Range, "0, 65535, 1")]
    public ushort TickTimeout { private set; get; } = 3;

    // Note: Ticks refers to the amount of applications of the effect
    [Export(PropertyHint.Range, "1, 32767, 1")]
    public short Ticks {
        private set {
            _ticks = value;
            _currentTick = value;
        }
        get => _ticks;
    }

    [Export(PropertyHint.Range, "0, 1, 0.01")]
    public double ApplicationChance { private set; get; } = 1.0;

    // Radius=0 means it's not an Aoe effect, Radius > 0 means IT IS an Aoe effect
    [Export(PropertyHint.Range, "0, 65535, 1")]
    public ushort Radius { private set; get; }

    // [ExportCategory("<NAME THIS ALSO>")]
    [Export] public EffectFlags Flags { private set; get; }

    public Timer? Timer { private set; get; }

    private StringName _id = new("");
    private short _ticks = 1;
    private short _currentTick = 1;


    // TODO: Rename to sth like: Apply? 
    public Timer? Apply() {
        if (!RNG.Roll(ApplicationChance)) {
            return null;
        }

        if (Timer is not null && !Timer.IsStopped()) {
            return Timer;
        }

        Timer = new Timer();
        Timer.Autostart = true;
        Timer.WaitTime = TickTimeout;

        if (_ticks == 1 /*|| IsImmediate()*/) {
            Timer.Ready += SetupPeriodicEffect;
        } else {
            Timer.Timeout += SetupPeriodicEffect;
        }

        return Timer;
    }

    private void SetupPeriodicEffect() {
        if (_currentTick > 0) {
            EmitSignalTick();
            _currentTick--;
        }

        if (_currentTick == 0) {
#if TOOLS
            if (Timer is null) {
                Logger.Core.Critical("Timer should not be null here, but it is!", true);
                return;
            }
#endif
            Timer.Stop();
            Timer.QueueFree();
            EmitSignalFinished();
        }
    }
}