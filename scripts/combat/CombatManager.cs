using Godot;
using System.Collections.Generic;
using RPG.global;
using RPG.scripts.effects;
using RPG.scripts.effects.components;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;
using RPG.world;

namespace RPG.scripts.combat;

// Note to self (delete me later): I did not finish with this file! Finish it now!



// I said now!

[GlobalClass]
public partial class CombatManager : Node {
    [Export] public CombatSystem CombatSystem { private set; get; } = new();

    // Tracks currently targeted entity by player.
    public Entity? TargetEntity { set; get; }

    private readonly StatSystem _statSystem = new();
    private CombatResources CombatResources => new(CombatSystem);

    // Keeps track of applied stacks for each `EffectComponent`, not `Effect`!
    // This has advantage of being able to define both StatEffectComponent and DamageEffectComponent on the same effect
    // and have the stacks be tracked separately.
    private readonly Dictionary<EffectComponent, EffectData> _appliedStacks = [];

    private class EffectData(ushort pStacks, Gizmo pGizmo, Timer? pTimer) {
        public ushort Stacks = pStacks;

        // ReSharper disable once UnusedMember.Local
        // It will be used for displaying Effects on UI later
        public readonly Gizmo Gizmo = pGizmo;
        public readonly Timer? Timer = pTimer;
    }

    public override void _Ready() {
        Entity entity = GetEntity();

        _statSystem.LinkFromInventory(entity.Armory);
        _statSystem.Link(entity.BaseStats);

        CombatSystem.Initialize(_statSystem.Total);
    }

    public void Cast(Gizmo pGizmo, Entity? pTarget) {
        SpellComponent? spellComponent = pGizmo.GetComponent<SpellComponent, ChainSpellComponent>();
        // Targets `pTarget` if it's not null otherwise takes current target selected by the player
        pTarget ??= TargetEntity;
            
        if (spellComponent is not null && (IsInstanceValid(pTarget) || spellComponent.IsAoe)) {
            SpellComponent.CastResult result = spellComponent.Cast(pGizmo);
            if (result == SpellComponent.CastResult.Ok) {
                ApplyEffectsToTarget(pGizmo, spellComponent.Effects, pTarget);
            }
        } else {
            Logger.Combat.Debug("No valid target to cast the spell.");
        }

#if DEBUG
        // Sanity check: since ChainSpellComponent and SpellComponent is weird, I need to warn myself to avoid making mistakes.
        // ChainSpellComponent will take priority over SpellComponent!
        if (spellComponent is ChainSpellComponent && pGizmo.GetComponent<SpellComponent>() is not null) {
            Logger.Combat.Warn(
                $"{nameof(Gizmo)} '{pGizmo.DisplayName}' has both {nameof(ChainSpellComponent)} and {nameof(SpellComponent)}. Spell will be cast through {nameof(ChainSpellComponent)} only and {nameof(SpellComponent)} will be ignored.");
        }
#endif
    }

    private void ApplyEffectsToTarget(Gizmo pEffectsOwner, Effect[] pEffects, Entity? pTarget) {
        foreach (Effect effect in pEffects) {
            ApplyEffectToTargetEntity(pEffectsOwner, effect, pTarget);
        }
    }

    private void ApplyEffectToTargetEntity(Gizmo pEffectOwner, Effect pEffect, Entity? pTarget) {
        // Entity? target = pEffect.IsTargetSelf() ? GetEntity() : pTarget;

        if (!IsInstanceValid(pTarget)) {
            Logger.Combat.Debug($"No valid target to apply effects.");
            return;
        }

        var areaEffectComponent = pEffect.GetComponent<AreaOfEffectComponent>();
        
        if (areaEffectComponent is not null) {
            foreach (Entity entityInRadius in pTarget.GetEntitiesInRadius(areaEffectComponent.Radius)) {
                Logger.Combat.Debug(
                    $"Applying effect to entity in Radius={areaEffectComponent.Radius} Entity={entityInRadius.Name} ({entityInRadius.GetType().Name})"
                );

                entityInRadius.CombatManager.ApplyEffect(pEffectOwner, pEffect, this);
            }
        }

        pTarget.CombatManager.ApplyEffect(pEffectOwner, pEffect, this);
    }

    private void ApplyEffect(Gizmo pEffectOwner, Effect pEffect, CombatManager pSource) {
        // NOTE:
        // Effect is a reference counted Resource. It means that if 2 different Entities receive the same effect it will not function properly.
        // For every application of Effect._currentTick must be separate.
        // As of writing this EffectComponents don't need to be duplicated so the flag is set to false.
        pEffect = (Effect)pEffect.Duplicate(false);

        Timer? timer = pEffect.Start();

        if (timer is not null) {
            AddChild(timer);
        }

        // By default, all effects are debuffs, which means:
        //      1) DamageEffectComponent deals damage, duh,
        //      2) StatEffectComponent "debuffs" (subtracts from) the stat
        // When Buff flag is set:
        //      1) DamageEffectComponent heals!!!
        //      2) StatEffectComponent "buffs" (adds to) the stat
        int effectCoefficient = pEffect.IsBuff() ? -1 : 1;

        var damageEffectComponent = pEffect.GetComponent<DamageEffectComponent>();

        if (damageEffectComponent is not null) {
            pEffect.Tick += () => OnDamageEffectTick(damageEffectComponent, pSource, effectCoefficient);
            pEffect.Finished += () => OnDamageEffectFinished(damageEffectComponent);

            if (_appliedStacks.TryGetValue(damageEffectComponent, out EffectData? oEffectData)) {
                if (oEffectData.Stacks < damageEffectComponent.MaxStacks) {
                    _appliedStacks[damageEffectComponent].Stacks++;
                }

                // Restart the timer
                oEffectData.Timer?.Start();
            } else {
                _appliedStacks.Add(damageEffectComponent, new EffectData(1, pEffectOwner, timer));
            }
        }

        var statEffectComponent = pEffect.GetComponent<StatEffectComponent>();

        if (statEffectComponent is not null) {
            pEffect.Tick += () => OnStatEffectTick(statEffectComponent, pSource, effectCoefficient);
            pEffect.Finished += () => OnDamageEffectFinished(statEffectComponent);

            if (_appliedStacks.TryGetValue(statEffectComponent, out EffectData? oEffectData)) {
                if (oEffectData.Stacks < statEffectComponent.MaxStacks) {
                    _appliedStacks[statEffectComponent].Stacks++;
                }
                
                // Restart the timer
                oEffectData.Timer?.Start();
            } else {
                _appliedStacks.Add(statEffectComponent, new EffectData(1, pEffectOwner, timer));
            }
        }
    }

    private void OnDamageEffectFinished(EffectComponent pEffect) {
        _appliedStacks.Remove(pEffect);
    }

    private void OnStatEffectFinished(EffectComponent pEffect) {
        _appliedStacks.Remove(pEffect);
    }

    private void OnDamageEffectTick(DamageEffectComponent pDamageEffect, CombatManager pSource, int pCoefficient) {
        double damage = CombatSystem.CalculateDamage(pDamageEffect, pSource.CombatSystem);

        damage *= _appliedStacks[pDamageEffect].Stacks * pCoefficient;

        CombatResources.ModifyHealth(damage);
    }

    // ReSharper disable UnusedParameter.Local
    private void OnStatEffectTick(StatEffectComponent pEffect, CombatManager pSource, int pCoefficient) { }


    private Entity GetEntity() {
        return GetParent<Entity>();
    }
}