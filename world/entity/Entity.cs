using Godot;
using Godot.Collections;
using RPG.scripts;
using RPG.scripts.combat;
using RPG.scripts.inventory;

namespace RPG.world.entity;

[GlobalClass]
public partial class Entity : Node3D {
    [Signal]
    public delegate void AboutToBeFreedEventHandler();

    [Signal]
    public delegate void BaseStatsAboutToChangeEventHandler(Stats? pOld, Stats? pNew);

    private Stats _baseStats = null!;

    [Export] public Faction MainFaction = null!;

    // Temporary Faction with no ID 
    public Faction? OverrideFaction;
    [Export] public Faction PersonalFaction = null!;

    [Export] public SpellManager SpellManager { private set; get; } = null!;
    [Export] public Inventory Armory { private set; get; } = null!;
    [Export] public Inventory SpellBook { private set; get; } = null!;

    [Export]
    public Stats BaseStats {
        private set {
            // TODO: check if this works:

            // This setter will be called by the editor when the object is created (way before `_Ready` and `_EnterTree`)
            // So the delay is a hack to allow child nodes to connect to the signal below.
            CallDeferred(nameof(EmitBaseStatsAboutToChanged), Variant.From(_baseStats), Variant.From(value));
            _baseStats = value;
        }
        get => _baseStats;
    }

    private void EmitBaseStatsAboutToChanged(Stats? pOld, Stats? pNew) {
        EmitSignalBaseStatsAboutToChange(pOld, pNew);
    }

    public override void _EnterTree() {
        TreeExiting += () => {
            if (IsQueuedForDeletion()) {
                EmitSignalAboutToBeFreed();
            }
        };
    }

    public bool IsEnemyOf(Entity pOther) {
        if (OverrideFaction is not null) {
            if (OverrideFaction.HasReputation(pOther.PersonalFaction.Id)) {
                return OverrideFaction.IsEnemyNoCheck(pOther.PersonalFaction.Id);
            }

            if (OverrideFaction.HasReputation(pOther.MainFaction.Id)) {
                return OverrideFaction.IsEnemyNoCheck(pOther.MainFaction.Id);
            }
        }

        if (PersonalFaction.HasReputation(pOther.PersonalFaction.Id)) {
            return PersonalFaction.IsEnemyNoCheck(pOther.PersonalFaction.Id);
        }

        if (PersonalFaction.HasReputation(pOther.MainFaction.Id)) {
            return PersonalFaction.IsEnemyNoCheck(pOther.MainFaction.Id);
        }

        if (MainFaction.HasReputation(pOther.PersonalFaction.Id)) {
            return MainFaction.IsEnemyNoCheck(pOther.PersonalFaction.Id);
        }

        if (MainFaction.HasReputation(pOther.MainFaction.Id)) {
            return MainFaction.IsEnemyNoCheck(pOther.MainFaction.Id);
        }

        return true;
    }

    public Entity? GetNearestEnemy() {
        ModelHolder modelHolder = GetModelHolder();

        Array<Entity> nodes = modelHolder.GetEntitiesInSight();

        Entity? closestEnemy = null;
        float shortestDistanceSquared = float.MaxValue;
        foreach (Entity entity in nodes) {
            if (!IsEnemyOf(entity)) {
                continue;
            }

            float distanceSquared = GlobalPosition.DistanceSquaredTo(entity.GlobalPosition);

            if (distanceSquared < shortestDistanceSquared) {
                shortestDistanceSquared = distanceSquared;
                closestEnemy = entity;
            }
        }

        return closestEnemy;
    }

    public World GetWorld() {
        return GetParent<World>();
    }

    public ModelHolder GetModelHolder() {
        return GetChild(0).GetChild<ModelHolder>(0);
    }
}