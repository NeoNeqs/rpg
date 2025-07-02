using Godot;
using System.Collections.Generic;
using RPG.global;
using RPG.scripts.effects;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;
using RPG.world;

namespace RPG.scripts.combat;

[GlobalClass]
public partial class CombatManager : Node {
    [Export] public CombatSystem CombatSystem { private set; get; } = new();

    // Tracks currently targeted entity by player.
    public Entity? TargetEntity { set; get; }

    private CombatData CombatData => new(CombatSystem);

    // Keeps track of applied stacks for each `EffectComponent`, not `Effect`!
    // This has advantage of being able to define both StatEffectComponent and DamageEffectComponent on the same effect
    // and have the stacks be tracked separately.
    private readonly Dictionary<StringName, Gizmo> _appliedEffects = [];

    private readonly StatSystem _statSystem = new();

    public override void _EnterTree() {
        Entity entity = GetEntity();

        _statSystem.LinkFromInventory(entity.Armory);

        // ReSharper disable RedundantLambdaParameterType
        entity.BaseStatsAboutToChange += (Stats? pOld, Stats? pNew) => {
            _statSystem.Unlink(pOld);
            _statSystem.Link(pNew);
        };
        // ReSharper restore RedundantLambdaParameterType

        CombatSystem.Initialize(_statSystem.Total);
    }

    public void Cast(Gizmo pGizmo, Entity? pTarget) {
        SpellComponent? spellComponent = pGizmo.GetComponent<SpellComponent, ChainSpellComponent>();
        // Targets `pTarget` if it's not null otherwise takes current target, selected by the player
        pTarget ??= TargetEntity;

        // ReSharper disable once UseNullPropagation
        if (spellComponent is not null) {
            SpellComponent.CastResult result = spellComponent.Cast(pGizmo);
            if (result == SpellComponent.CastResult.Ok) {
                ApplyEffectsToTarget(pGizmo, spellComponent.Effects, pTarget, spellComponent.IsAoe);
            }
        }

#if TOOLS
        // Sanity check: since ChainSpellComponent and SpellComponent is weird, I need to warn myself to avoid making mistakes.
        // ChainSpellComponent will take priority over SpellComponent!
        if (spellComponent is ChainSpellComponent && pGizmo.GetComponent<SpellComponent>() is not null) {
            Logger.Combat.Warn(
                $"{nameof(Gizmo)} '{pGizmo.DisplayName}' has both {nameof(ChainSpellComponent)} and {nameof(SpellComponent)}. Spell will be cast through {nameof(ChainSpellComponent)} only and {nameof(SpellComponent)} will be ignored.");
        }
#endif
    }

    private void ApplyEffectsToTarget(Gizmo pEffectsOwner, Effect[] pEffects, Entity? pTarget, bool pIsAoe) {
        foreach (Effect effect in pEffects) {
            ApplyEffectToTargetEntity(pEffectsOwner, effect, pTarget, pIsAoe);
        }
    }

    private void ApplyEffectToTargetEntity(Gizmo pEffectOwner, Effect pEffect, Entity? pTarget, bool pIsAoe) {
        // Entity? target = pEffect.IsTargetSelf() ? GetEntity() : pTarget;

        if (!IsInstanceValid(pTarget)) {
            Logger.Combat.Debug($"No valid target to apply effects.");
            return;
        }

        if (pIsAoe) {
            foreach (Entity entityInRadius in pTarget.GetEntitiesInRadius(pEffect.Radius)) {
                entityInRadius.CombatManager.ApplyEffect(pEffectOwner, pEffect, this);
            }
        } else {
            pTarget.CombatManager.ApplyEffect(pEffectOwner, pEffect, this);
        }
    }

    private void ApplyEffect(Gizmo pEffectOwner, Effect pEffect, CombatManager pSource) {
        // NOTE:
        // Effect is a reference counted Resource. It means that if 2 different Entities receive the same effect it will not function properly.
        // For every application of the Effect, Effect._currentTick must be separate.
        pEffect = (Effect)pEffect.Duplicate(false);

        Timer? timer = pEffect.Apply();

        if (timer is null) {
            return;
        }

        switch (pEffect) {
            case DamageEffect damageEffect:
                // don't connect signals again
                if (!_appliedEffects.ContainsKey(damageEffect.Id)) {
                    damageEffect.Tick += () => OnDamageEffectTick(damageEffect, pSource);
                    damageEffect.Finished += () => OnDamageEffectFinished(damageEffect, pSource);
                    _appliedEffects[damageEffect.Id] = pEffectOwner;
                }

                damageEffect.Stack();
                break;
            case StatEffect statEffect:
                // don't connect signals again
                if (!_appliedEffects.ContainsKey(statEffect.Id)) {
                    statEffect.Tick += () => OnStatEffectTick(statEffect, pSource);
                    statEffect.Finished += () => OnStatEffectFinished(statEffect, pSource);
                    _appliedEffects[statEffect.Id] = pEffectOwner;
                }

                statEffect.Stack();
                break;
        }

        // Must be added after connecting to `Tick` and `Finished` signals, otherwise no ticking will happen
        if (!timer.IsInsideTree()) {
            AddChild(timer);
        }

        // if (pEffect is StackingEffect stackingEffect) {
        //     if (!_appliedStacks.TryGetValue(pEffect.Id, out EffectData? oEffectData)) {
        //         _appliedStacks.Add(pEffect.Id, new EffectData(pEffectOwner, stackingEffect, timer));
        //     } else {
        //         oEffectData.Effect.Stack();
        //         oEffectData.Timer?.Start();
        //     }
        // }
        // var damageEffectComponent = pEffect.GetComponent<DamageEffectComponent>();
        //
        // if (damageEffectComponent is not null) {
        //     pEffect.Tick += () => OnDamageEffectTick(damageEffectComponent, pSource, pEffect.IsBuff());
        //     pEffect.Finished += () => OnDamageEffectFinished(pSource, damageEffectComponent);
        //
        //     if (_appliedStacks.TryGetValue(damageEffectComponent, out EffectData? oEffectData)) {
        //         if (oEffectData.Stacks < damageEffectComponent.MaxStacks) {
        //             _appliedStacks[damageEffectComponent].Stacks++;
        //         }
        //
        //         // Restart the timer
        //         oEffectData.Timer?.Start();
        //     } else {
        //         _appliedStacks.Add(damageEffectComponent, new EffectData(1, pEffectOwner, null));
        //     }
        //
        //     Logger.Combat.Debug(
        //         $"Entity {pSource.GetEntity().Name} applied damage effect {damageEffectComponent.DisplayName} x{_appliedStacks[damageEffectComponent].Stacks} to entity {GetEntity().Name}.");
        // }
        //
        // var statEffectComponent = pEffect.GetComponent<StatEffectComponent>();
        //
        // if (statEffectComponent is not null) {
        //     pEffect.Tick += () => OnStatEffectTick(statEffectComponent, pSource, pEffect.IsBuff());
        //     pEffect.Finished += () => OnStatEffectFinished(pSource, statEffectComponent);
        //
        //     if (_appliedStacks.TryGetValue(statEffectComponent, out EffectData? oEffectData)) {
        //         if (oEffectData.Stacks < statEffectComponent.MaxStacks) {
        //             _appliedStacks[statEffectComponent].Stacks++;
        //         }
        //
        //         // Restart the timer
        //         oEffectData.Timer?.Start();
        //     } else {
        //         _appliedStacks.Add(statEffectComponent, new EffectData(1, pEffectOwner, timer));
        //     }
        //
        //     Logger.Combat.Debug(
        //         $"Entity {pSource.GetEntity().Name} applied stat effect {statEffectComponent.DisplayName} x{_appliedStacks[statEffectComponent].Stacks} to entity {GetEntity().Name}.");
        // }
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
        _appliedEffects.Remove(pDamageEffect.Id);
    }

    private void OnStatEffectFinished(StatEffect pStatEffect, CombatManager pSource) {
        Logger.Combat.Debug(
            $"Stat effect {pStatEffect.DisplayName} removed from {GetEntity().Name} applied by {pSource.GetEntity().Name}");
        _appliedEffects.Remove(pStatEffect.Id);
    }

    private Entity GetEntity() {
        return GetParent<Entity>();
    }
}