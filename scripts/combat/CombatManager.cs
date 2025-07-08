using System;
using Godot;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RPG.global;
using RPG.global.singletons;
using RPG.scripts.effects;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;
using RPG.world;
using Entity = RPG.world.entity.Entity;

namespace RPG.scripts.combat;

[GlobalClass]
public partial class CombatManager : Node, IContainer<(Gizmo, Effect)> {
    [Signal]
    public delegate void AppliedEffectEventHandler(Gizmo pEffectOwner, Effect pEffect);

    [Signal]
    public delegate void RemovedEffectEventHandler(Gizmo pEffectOwner, Effect pEffect, int pIndex);

    [Signal]
    public delegate void TargetChangedEventHandler(Entity pSource, Entity? pNewTarget);

    [Export] public CombatSystem CombatSystem { private set; get; } = new();

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
    private CombatData CombatData => new(CombatSystem);

    // Keeps track of applied stacks for each `EffectComponent`, not `Effect`!
    // This has advantage of being able to define both StatEffectComponent and DamageEffectComponent on the same effect
    // and have the stacks be tracked separately.
    private readonly OrderedDictionary<StringName, (Gizmo, Effect)> _appliedEffects = [];
    
    private readonly StatLinker _statLinker = new();

    public override void _EnterTree() {
        Entity entity = GetEntity();

        _statLinker.LinkFromInventory(entity.Armory);

        entity.BaseStatsAboutToChange += (Stats? pOld, Stats? pNew) => {
            _statLinker.Unlink(pOld);
            _statLinker.Link(pNew);
        };

        CombatSystem.Initialize(_statLinker.Total);
    }

    public ReadOnlyDictionary<StringName, (Gizmo, Effect)> GetAppliedEffects() {
        return _appliedEffects.AsReadOnly<StringName, (Gizmo, Effect)>();
    }

    public void Cast(Gizmo pGizmo, Entity? pTarget) {
        SpellComponent? spellComponent = pGizmo.GetComponent<SpellComponent, ChainSpellComponent>();

        if (spellComponent is null) {
            return;
        }

        // Targets `pTarget` if it's not null otherwise takes current target, selected by the player
        pTarget ??= TargetEntity;
        if (spellComponent.IsOnCooldown()) {
            return;
        }

        ApplyEffectsToTarget(pGizmo, spellComponent.GetEffects(), pTarget);
        HandleLinkedSpells(pGizmo, spellComponent);
        // IMPORTANT: SpellComponent.Cast must be the last thing executed here so that UI can display spells 
        //            and effects correctly.
        spellComponent.Cast(pGizmo);
    }

    private void ApplyEffectsToTarget(Gizmo pEffectsOwner, Effect[] pEffects, Entity? pTarget) {
        foreach (Effect effect in pEffects) {
            ApplyEffectToTargetEntity(pEffectsOwner, effect, pTarget);
        }
    }

    private void ApplyEffectToTargetEntity(Gizmo pEffectOwner, Effect pEffect, Entity? pTarget) {
        if (pEffect.IsTargetSelfOnly()) {
            pTarget = GetEntity();
            // pIsAoe = pEffect.IsTargetAllies();
        }

        if (!IsInstanceValid(pTarget)) {
            Logger.Combat.Debug("No valid target to apply effects.");
            return;
        }

        if (pEffect.IsAoe) {
            foreach (Entity entityInRadius in pTarget.GetEntitiesInRadius(pEffect.Radius, [])) {
                bool isAlly = entityInRadius.Faction == GetEntity().Faction;

                if ((isAlly && pEffect.IsTargetAllies()) || (!isAlly && !pEffect.IsTargetAllies())) {
                    entityInRadius.CombatManager.ApplyEffect(pEffectOwner, pEffect, this);
                }
            }
        } else {
            pTarget.CombatManager.ApplyEffect(pEffectOwner, pEffect, this);
        }
    }

    private void ApplyEffect(Gizmo pEffectOwner, Effect pEffect, CombatManager pSource) {
        // IMPORTANT: Since Effects are duplicated pEffect parameter is not the same as the one stored in _appliedEffects!

        if (_appliedEffects.TryGetValue(pEffect.Id, out ValueTuple<Gizmo, Effect> actualData)) {
            (Gizmo _, Effect actualEffect) = actualData;

            actualEffect.Refresh();
            return;
        }

        // IMPORTANT: Effects must be duplicated so that their logic is separate for each Entity that could use it.
        pEffect = (Effect)pEffect.Duplicate(false);

        Timer? timer = pEffect.Start();

        if (timer is null) {
            return;
        }

#if TOOLS
        if (!timer.IsStopped()) {
            Logger.Combat.Debug("This should not happen!", true);
        }
#endif

        foreach (StringName excludingEffectId in pEffect.ExcludingEffects) {
            if (!_appliedEffects.TryGetValue(excludingEffectId, out (Gizmo, Effect) value)) {
                continue;
            }

            (Gizmo effectOwner, Effect effect) = value;
            effect.CleanupAndFinish();
            var index = _appliedEffects.IndexOf(excludingEffectId);
            _appliedEffects.Remove(excludingEffectId);

            EmitSignalRemovedEffect(effectOwner, effect, index);
        }

        switch (pEffect) {
            case DamageEffect damageEffect:
                damageEffect.Tick += () => OnDamageEffectTick(damageEffect, pSource);
                damageEffect.Finished += () => OnDamageEffectFinished(damageEffect, pSource);

                _appliedEffects[damageEffect.Id] = (pEffectOwner, damageEffect);
                EmitSignalAppliedEffect(pEffectOwner, damageEffect);

                break;
            case StatEffect statEffect:
                statEffect.Tick += () => OnStatEffectTick(statEffect, pSource);
                statEffect.Finished += () => OnStatEffectFinished(statEffect, pSource);

                _appliedEffects[statEffect.Id] = (pEffectOwner, statEffect);
                EmitSignalAppliedEffect(pEffectOwner, statEffect);

                break;
        }

        // Must be added after connecting to `Tick` and `Finished` signals, otherwise no ticking will happen
        if (!timer.IsInsideTree()) {
            AddChild(timer);
        }
    }

    private void OnDamageEffectTick(DamageEffect pDamageEffect, CombatManager pSource) {
        double damage = CombatSystem.CalculateDamage(pDamageEffect, pSource.CombatSystem);

        damage *= pDamageEffect.CurrentStacks;

        // if (pIsBuff) {
        // damage *= -1;
        // }

        CombatData.ModifyHealth(damage);
    }

    // ReSharper disable UnusedParameter.Local
    private void OnStatEffectTick(StatEffect pEffect, CombatManager pSource) { }

    private void OnDamageEffectFinished(DamageEffect pDamageEffect, CombatManager pSource) {
        Logger.Combat.Debug(
            $"Damage effect {pDamageEffect.DisplayName} removed from {GetEntity().Name} applied by {pSource.GetEntity().Name}");

        
        (Gizmo effectOwner, Effect effect) = _appliedEffects[pDamageEffect.Id];

        var index = _appliedEffects.IndexOf(pDamageEffect.Id);
        
        _appliedEffects.Remove(pDamageEffect.Id);
        EmitSignalRemovedEffect(effectOwner, effect, index);
    }

    private void OnStatEffectFinished(StatEffect pStatEffect, CombatManager pSource) {
        Logger.Combat.Debug(
            $"Stat effect {pStatEffect.DisplayName} removed from {GetEntity().Name} applied by {pSource.GetEntity().Name}");

        (Gizmo effectOwner, Effect effect) = _appliedEffects[pStatEffect.Id];

        // TODO: remember do subtract the stats
        
        var index = _appliedEffects.IndexOf(pStatEffect.Id);

        _appliedEffects.Remove(pStatEffect.Id);
        EmitSignalRemovedEffect(effectOwner, effect, index);
    }

    private static void HandleLinkedSpells(Gizmo pSource, SpellComponent pSpellComponent) {
        foreach (StringName linkedSpell in pSpellComponent.LinkedSpells) {
            Gizmo? spell = ResourceDB.GetSpell(linkedSpell);
            if (spell is null) {
                continue;
            }

            SpellComponent? spellComponent = spell.GetComponent<SpellComponent, ChainSpellComponent>();
            if (spellComponent is null) {
                continue;
            }

            if (spell.Id == pSource.Id) {
                continue;
            }

            spellComponent.LastCastTimeMicroseconds = Time.GetTicksUsec();
            spellComponent.EmitCastComplete(spellComponent.CooldownSeconds * 1_000_000);
        }
    }

    private Entity GetEntity() {
        return GetParent<Entity>();
    }

    public int GetSize() {
        return _appliedEffects.Count;
    }
    
    public (Gizmo, Effect) GetAt(int pIndex) {
        return _appliedEffects.GetAt(pIndex).Value;
    }
}