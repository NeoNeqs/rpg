using System.Collections.Generic;
using global::RPG.global;
using Godot;
using RPG.global.enums;
using RPG.scripts.effects;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;
using RPG.world;
using RPG.world.entity;

namespace RPG.scripts.combat;

[GlobalClass]
public partial class SpellManager : Node, IContainer<AppliedEffect> {
    [Signal]
    public delegate void AppliedEffectEventHandler(AppliedEffect pAppliedEffect);

    [Signal]
    public delegate void RemovedEffectEventHandler(AppliedEffect pAppliedEffect, int pIndex);

    [Signal]
    public delegate void TargetChangedEventHandler(Entity pSource, Entity? pNewTarget);

    private readonly OrderedDictionary<StringName, AppliedEffect> _appliedEffects = [];
    private readonly StatLinker _statLinker = new();

    private Entity? _targetEntity;

    [Export] public StatCalculator StatCalculator { private set; get; } = new();

    // Tracks currently targeted entity by player.
    public Entity? TargetEntity {
        set {
            if (_targetEntity != value) {
                EmitSignalTargetChanged(GetEntity(), value);
            }

            _targetEntity = value;
        }
        get => _targetEntity;
    }

    private CombatData CombatData => new(StatCalculator);

    public int GetSize() {
        return _appliedEffects.Count;
    }

    public AppliedEffect GetAt(int pIndex) {
        return _appliedEffects.GetAt(pIndex).Value;
    }

    public override void _EnterTree() {
        Entity entity = GetEntity();

        _statLinker.LinkFromInventory(entity.Armory);

        entity.BaseStatsAboutToChange += (Stats? pOld, Stats? pNew) => {
            _statLinker.Unlink(pOld);
            _statLinker.Link(pNew);
        };

        StatCalculator.Initialize(_statLinker);
    }

    public CastResult Cast(Gizmo pGizmo) {
        SpellComponent? spellComponent = pGizmo.GetComponent<SpellComponent, SequenceSpellComponent>();

        if (spellComponent is null) {
            return CastResult.NoSpellComponent;
        }

        if (spellComponent.IsOnCooldown()) {
            return CastResult.OnCooldown;
        }

        return CastNoCDCheck(pGizmo, spellComponent);
    }

    public CastResult CastNoCDCheck(Gizmo pGizmo, SpellComponent pSpellComponent) {
        Entity thisEntity = GetEntity();
        Entity? potentialTarget = TargetEntity;

        if (potentialTarget is null) {
            if (pSpellComponent.RequiresEnemyTarget()) {
                Entity? nearestEnemy = thisEntity.GetNearestEnemy();

                if (nearestEnemy is null) {
                    return CastResult.NoValidTarget;
                }

                TargetEntity = nearestEnemy;
                potentialTarget = nearestEnemy;
            } else {
                // Friendly spell, target itself
                potentialTarget = thisEntity;
                TargetEntity = thisEntity;
            }
        }

        bool isHarmful = thisEntity.IsEnemyOf(potentialTarget);

        if (pSpellComponent.RequiresEnemyTarget() && !isHarmful) {
            return CastResult.SpellWrongTarget;
        }

        if (!pSpellComponent.RequiresEnemyTarget() && isHarmful) {
            return CastResult.SpellWrongTarget;
        }

        CastResult castResult = ApplyEffectsToTarget(pGizmo, pSpellComponent.GetEffects(), potentialTarget, isHarmful);

        if (castResult.IsError()) {
            return castResult;
        }

        HandleLinkedSpells(pGizmo, pSpellComponent);
        // IMPORTANT: SpellComponent.Cast must be the last thing executed here so that UI can display spells 
        //            and effects correctly.
        pSpellComponent.Cast();

        return castResult;
    }

    private CastResult ApplyEffectsToTarget(Gizmo pEffectsOwner, Effect[] pEffects, Entity pTarget, bool pIsHarmful) {
        foreach (Effect effect in pEffects) {
            // IMPORTANT: Effects must be duplicated so that their logic is separate for each Entity that could use it.
            var duplicatedEffect = (Effect)effect.Duplicate(false);

            CastResult result = ApplyEffectToTarget(pEffectsOwner, duplicatedEffect, pTarget, pIsHarmful);
            // As of writing this, ApplyEffectToTarget will not fail at any path, this might get changed in the future. 
            if (result.IsError()) {
                return result;
            }
        }

        return CastResult.Ok;
    }

    private CastResult ApplyEffectToTarget(Gizmo pEffectOwner, Effect pEffect, Entity pTarget, bool pIsHarmful) {
        Entity thisEntity = GetEntity();

        if (pEffect.IsAoe()) {
            World world = GetEntity().GetWorld();

            pTarget = world.CreateTempDummyEntity(world.GetDecal().GlobalPosition);
            pEffect.Finished += () => { pTarget.QueueFree(); };
        }

        CastResult result = pTarget.SpellManager.ApplyEffect(pEffectOwner, pEffect, thisEntity, pIsHarmful);

        if (pEffect.Behavior is not null) {
            AppliedEffect appliedEffect = _appliedEffects[pEffect.Id];
            pEffect.Behavior.Run(thisEntity, appliedEffect, pTarget, pIsHarmful);
        }

        if (!pEffect.Timer.IsInsideTree()) {
            pTarget.SpellManager.AddChildOwned(pEffect.Timer, pTarget.SpellManager);
        }

        return result;
    }

    public CastResult ApplyEffect(Gizmo pEffectOwner, Effect pEffect, Entity pSourceEntity, bool pIsHarmful) {
        if (pEffect.TargetsEnemies() && !pIsHarmful) {
            return CastResult.EffectIgnored;
        }

        if (!pEffect.TargetsEnemies() && pIsHarmful) {
            return CastResult.EffectIgnored;
        }

        if (!RNG.Roll(pEffect.ApplicationChance)) {
            return CastResult.Missed;
        }

        HandleExcludingEffects(pEffect);

        if (_appliedEffects.TryGetValue(pEffect.Id, out AppliedEffect? pAppliedEffect)) {
            // IMPORTANT: Since Effects are duplicated pEffect parameter is not the same as the one stored in _appliedEffects!

            EmitSignalAppliedEffect(pAppliedEffect);
            pAppliedEffect.Effect.Refresh();
            return CastResult.OkRefreshed;
        }

        switch (pEffect) {
            case DamageEffect damageEffect:
                damageEffect.Tick += () => OnDamageEffectTick(damageEffect, pSourceEntity.SpellManager);
                damageEffect.Finished += () => OnDamageEffectFinished(damageEffect, pSourceEntity.SpellManager);

                var damageData = new AppliedEffect(pEffectOwner, pEffect);
                _appliedEffects[damageEffect.Id] = damageData;
                EmitSignalAppliedEffect(damageData);

                break;
            case StatEffect statEffect:
                statEffect.Tick += () => OnStatEffectTick(statEffect, pSourceEntity.SpellManager);
                statEffect.Finished += () => OnStatEffectFinished(statEffect, pSourceEntity.SpellManager);

                var statData = new AppliedEffect(pEffectOwner, pEffect);
                _appliedEffects[statEffect.Id] = statData;
                EmitSignalAppliedEffect(statData);

                break;
        }

        return CastResult.Ok;
    }


    private void OnDamageEffectTick(DamageEffect pDamageEffect, SpellManager pSource) {
        double damage = StatCalculator.CalculateDamage(pDamageEffect, pSource.StatCalculator);

        damage *= pDamageEffect.CurrentStacks;

        // if (pIsBuff) {
        // damage *= -1;
        // }

        CombatData.ModifyHealth(damage);
    }

    private void OnStatEffectTick(StatEffect pEffect, SpellManager pSource) { }

    private void OnDamageEffectFinished(DamageEffect pDamageEffect, SpellManager pSource) {
        // Logger.Combat.Debug(
        // $"Damage effect {pDamageEffect.DisplayName} removed from {GetEntity().Name} applied by {pSource.GetEntity().Name}");


        AppliedEffect data = _appliedEffects[pDamageEffect.Id];
        int index = _appliedEffects.IndexOf(pDamageEffect.Id);

        _appliedEffects.Remove(pDamageEffect.Id);
        EmitSignalRemovedEffect(data, index);
    }

    private void OnStatEffectFinished(StatEffect pStatEffect, SpellManager pSource) {
        Logger.Combat.Debug(
            $"Stat effect {pStatEffect.DisplayName} removed from {GetEntity().Name} applied by {pSource.GetEntity().Name}");

        AppliedEffect data = _appliedEffects[pStatEffect.Id];

        // TODO: remember do subtract the stats


        int index = _appliedEffects.IndexOf(pStatEffect.Id);

        _appliedEffects.Remove(pStatEffect.Id);
        EmitSignalRemovedEffect(data, index);
    }

    private void HandleLinkedSpells(Gizmo pSource, SpellComponent pSpellComponent) {
        foreach (GizmoStack gizmoStack in GetEntity().SpellBook.Gizmos) {
            Gizmo? gizmo = gizmoStack.Gizmo;

            if (gizmo is null) {
                continue;
            }

            SpellComponent? spellComponent = gizmo.GetComponent<SpellComponent, SequenceSpellComponent>();
            if (spellComponent is null) {
                continue;
            }

            foreach (StringName linkedSpellId in pSpellComponent.GetLinkedSpells()) {
                if (linkedSpellId != gizmo.Id || linkedSpellId == pSource.Id) {
                    continue;
                }

                spellComponent.BaseCast();
            }
        }
    }

    private void HandleExcludingEffects(Effect pEffect) {
        foreach (StringName excludingEffectId in pEffect.ExcludingEffects) {
            if (!_appliedEffects.TryGetValue(excludingEffectId, out AppliedEffect? data)) {
                continue;
            }

            int index = _appliedEffects.IndexOf(excludingEffectId);
            _appliedEffects.Remove(excludingEffectId);

            EmitSignalRemovedEffect(data, index);

            data.Effect.CleanupAndFinish();
        }
    }

    private Entity GetEntity() {
        return GetParent<Entity>();
    }

    // public ReadOnlyDictionary<StringName, (Gizmo, Effect)> GetAppliedEffects() {
    //     return _appliedEffects.AsReadOnly<StringName, (Gizmo, Effect)>();
    // }
}