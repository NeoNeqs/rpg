using Godot;
using System.Collections.Generic;
using RPG.global;
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

    private Entity? _targetEntity;
    private CombatData CombatData => new(StatCalculator);
    private readonly StatLinker _statLinker = new();

    private readonly OrderedDictionary<StringName, AppliedEffect> _appliedEffects = [];

    public override void _EnterTree() {
        Entity entity = GetEntity();

        _statLinker.LinkFromInventory(entity.Armory);

        entity.BaseStatsAboutToChange += (Stats? pOld, Stats? pNew) => {
            _statLinker.Unlink(pOld);
            _statLinker.Link(pNew);
        };

        StatCalculator.Initialize(_statLinker.Total);
    }

    public CastResult Cast(Gizmo pGizmo) {
        SpellComponent? spellComponent = pGizmo.GetComponent<SpellComponent, ChainSpellComponent>();

        if (spellComponent is null) {
            return CastResult.NoSpellComponent;
        }

        if (spellComponent.IsOnCooldown()) {
            return CastResult.OnCooldown;
        }

        return CastNoCDCheck(pGizmo, spellComponent);
    }

    public CastResult CastNoCDCheck(Gizmo pGizmo, SpellComponent pSpellComponent) {
        CastResult castResult = ApplyEffectsToTarget(pGizmo, pSpellComponent.GetEffects());
        
        HandleLinkedSpells(pGizmo, pSpellComponent);
        // IMPORTANT: SpellComponent.Cast must be the last thing executed here so that UI can display spells 
        //            and effects correctly.
        pSpellComponent.Cast(pGizmo);

        return castResult;
    }

    private CastResult ApplyEffectsToTarget(Gizmo pEffectsOwner, Effect[] pEffects) {
        Entity thisEntity = GetEntity();

        foreach (Effect effect in pEffects) {
            if (TargetEntity is not null) {
                if (effect.NeedsEnemyTarget() && thisEntity.Faction == TargetEntity.Faction) {
                    return CastResult.WrongTarget;
                }

                if (effect.NeedsFriendlyTarget() && thisEntity.Faction != TargetEntity.Faction) {
                    return CastResult.WrongTarget;
                }
            }
        }

        foreach (Effect effect in pEffects) {
            CastResult result = ApplyEffectToTarget(pEffectsOwner, effect);
            if (result != CastResult.Ok) {
                return result;
            }
        }

        return CastResult.Ok;
    }

    private CastResult ApplyEffectToTarget(Gizmo pEffectOwner, Effect pEffect) {
        // IMPORTANT: Effects must be duplicated so that their logic is separate for each Entity that could use it.
        pEffect = (Effect)pEffect.Duplicate(false);

        Entity thisEntity = GetEntity();
        Entity? potentialTarget = TargetEntity;

        if (potentialTarget is null) {
            if (pEffect.IsAoe()) {
                World world = GetEntity().GetWorld();

                potentialTarget = world.CreateTempDummyEntity(world.GetDecal().GlobalPosition);
                pEffect.Finished += () => {
                    potentialTarget.QueueFree();
                }; 
                
            } else if (pEffect.NeedsEnemyTarget()) {
                // potentialTarget = world.FindNearestEnemy() // Shape cast a cone ?
                // TargetEntity = potentialTarget;
                potentialTarget = new Entity();
            } else if (pEffect.NeedsFriendlyTarget()) {
                potentialTarget = thisEntity;
                TargetEntity = thisEntity;
            } else {
                return CastResult.TargetingError;
            }
        }

        Timer timer = potentialTarget.SpellManager.ApplyEffect(pEffectOwner, pEffect, this);
        
        if (pEffect.Behavior is not null) {
            pEffect.Behavior.Run(GetEntity(), pEffectOwner, pEffect, potentialTarget);
        } else {
            potentialTarget.SpellManager.AddChild(timer);
        }

        return CastResult.Ok;
    }

    public Timer ApplyEffect(Gizmo pEffectOwner, Effect pEffect, SpellManager pSource) {
        // if (!RNG.Roll(pEffect.ApplicationChance)) {
        //     return false;
        // }

        HandleExcludingEffects(pEffect);

        if (_appliedEffects.TryGetValue(pEffect.Id, out AppliedEffect? pAppliedEffect)) {
            // IMPORTANT: Since Effects are duplicated pEffect parameter is not the same as the one stored in _appliedEffects!

            EmitSignalAppliedEffect(pAppliedEffect);
            pAppliedEffect.Effect.Refresh();
            return pAppliedEffect.Effect.Timer;
        }

        switch (pEffect) {
            case DamageEffect damageEffect:
                damageEffect.Tick += () => OnDamageEffectTick(damageEffect, pSource);
                damageEffect.Finished += () => OnDamageEffectFinished(damageEffect, pSource);

                var damageData = new AppliedEffect(pEffectOwner, pEffect);
                _appliedEffects[damageEffect.Id] = damageData;
                EmitSignalAppliedEffect(damageData);

                break;
            case StatEffect statEffect:
                statEffect.Tick += () => OnStatEffectTick(statEffect, this);
                statEffect.Finished += () => OnStatEffectFinished(statEffect, this);
                
                var statData = new AppliedEffect(pEffectOwner, pEffect);
                _appliedEffects[statEffect.Id] = statData;
                EmitSignalAppliedEffect(statData);

                break;
        }

        return pEffect.TryStart();
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
        GD.Print(this);
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

            SpellComponent? spellComponent = gizmo.GetComponent<SpellComponent, ChainSpellComponent>();
            if (spellComponent is null) {
                continue;
            }

            foreach (StringName linkedSpellId in pSpellComponent.LinkedSpells) {
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

    public int GetSize() {
        return _appliedEffects.Count;
    }

    public AppliedEffect GetAt(int pIndex) {
        return _appliedEffects.GetAt(pIndex).Value;
    }

    // public ReadOnlyDictionary<StringName, (Gizmo, Effect)> GetAppliedEffects() {
    //     return _appliedEffects.AsReadOnly<StringName, (Gizmo, Effect)>();
    // }
}