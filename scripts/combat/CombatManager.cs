using Godot;
using Godot.Collections;
using RPG.global;
using RPG.scripts.effects;
using RPG.scripts.effects.components;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;
using RPG.world;

namespace RPG.scripts.combat;

[GlobalClass]
public partial class CombatManager : Node {
    public Entity? TargetEntity;

    [Export] public required CombatSystem CombatSystem;
    private readonly StatSystem _statSystem = new();
    private readonly CombatResources _combatResources = new();
    private readonly Dictionary<EffectComponent, ushort> _appliedStacks = [];


    public enum CastResult {
        Casted,
        OnCooldown,
        IsAoe,
    }
    public override void _Ready() {
        Entity entity = GetEntity();

        entity.Armory.GizmoChanged += LinkGizmoAttributes;
        entity.Armory.GizmoAboutToChange += UnlinkGizmoAttributes;

        _statSystem.Link(entity.BaseStats);
        LinkEntityArmory();

        CombatSystem.Initialize(_statSystem.Total);
        _combatResources.Initialize(CombatSystem);
    }


    public void Cast(Gizmo pGizmo) {
        var chainSpellComponent = pGizmo.GetComponent<ChainSpellComponent>();

        if (chainSpellComponent is not null) {
            Effect[] effects = CastChainSpell(pGizmo, chainSpellComponent);
            ApplyEffectsToTarget(effects);
        }

        var spellComponent = pGizmo.GetComponent<SpellComponent>();

        if (spellComponent is not null) {
            Effect[] effects = CastSpell(pGizmo, spellComponent);
            ApplyEffectsToTarget(effects);
        }
    }

    private void ApplyEffectsToTarget(Effect[] pEffects) {
        foreach (Effect effect in pEffects) {
            ApplyEffectToTargetEntity(effect);
        }
    }

    private void ApplyEffectToTargetEntity(Effect pEffect) {
        Entity? target = pEffect.IsTargetSelf() ? GetEntity() : TargetEntity;

        if (target is null || !IsInstanceValid(target)) {
            return;
        }

        var areaEffectComponent = pEffect.GetComponent<AreaOfEffectComponent>();
        if (areaEffectComponent is not null) {
            foreach (Entity entityInRadius in target.GetEntitiesInRadius(areaEffectComponent.Radius)) {
                entityInRadius.CombatManager.ApplyEffect(pEffect, this);
                Logger.Core.Debug($"{entityInRadius.GetType().Name} ({entityInRadius.Name})");
            }
        }

        target.CombatManager.ApplyEffect(pEffect, this);
    }

    private void ApplyEffect(Effect pEffect, CombatManager pAttacker) {
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
            pEffect.Tick += () => OnDamageEffectTick(damageEffectComponent, pAttacker, effectCoefficient);
            pEffect.Finished += () => OnDamageEffectFinished(damageEffectComponent);

            if (_appliedStacks.TryGetValue(damageEffectComponent, out ushort value)) {
                if (value < damageEffectComponent.MaxStacks) {
                    _appliedStacks[damageEffectComponent] = ++value;
                }
            } else {
                _appliedStacks.Add(damageEffectComponent, 1);
            }
        }

        var statEffectComponent = pEffect.GetComponent<StatEffectComponent>();
        if (statEffectComponent is not null) {
            pEffect.Tick += () => OnStatEffectTick(statEffectComponent, pAttacker, effectCoefficient);
            pEffect.Finished += () => OnDamageEffectFinished(statEffectComponent);

            if (_appliedStacks.TryGetValue(statEffectComponent, out ushort value)) {
                if (value < statEffectComponent.MaxStacks) {
                    _appliedStacks[statEffectComponent] = ++value;
                }
            } else {
                _appliedStacks.Add(statEffectComponent, 1);
            }
        }
    }

    private void OnDamageEffectFinished(EffectComponent pEffect) {
        _appliedStacks.Remove(pEffect);
    }

    private void OnStatEffectFinished(EffectComponent pEffect) {
        _appliedStacks.Remove(pEffect);
    }


    public bool HasTarget() {
        return TargetEntity is not null && IsInstanceValid(TargetEntity);
    }

    private static Effect[] CastSpell(Gizmo pSource, SpellComponent pSpellComponent) {
        if (pSpellComponent.IsOnCooldown()) {
            Logger.Combat.Debug($"Spell {pSource.DisplayName} is still on cooldown");
            return [];
        }

        ulong cooldownInMicroSeconds = pSpellComponent.CooldownSeconds * 1_000_000;
        pSource.EmitCasted(cooldownInMicroSeconds);

        pSpellComponent.Cast();
        return pSpellComponent.Effects;
    }

    private static Effect[] CastChainSpell(Gizmo pSource, ChainSpellComponent pChainSpellComponent) {
        Gizmo currentSpell = pChainSpellComponent.GetCurrentSpell() ?? pSource;

        if (pChainSpellComponent.IsOnCooldown()) {
            Logger.Combat.Debug($"Spell '{pSource.DisplayName}' is still on cooldown");
            return [];
        }

        bool castResult = pChainSpellComponent.Cast();
        if (castResult) {
            CompleteChainCast(pSource, pChainSpellComponent);
        } else {
            var spellComponent = currentSpell.GetComponent<SpellComponent>();
            if (spellComponent is null) {
                Logger.Combat.Debug($"Spell '{pSource.DisplayName}' is missing spell component");
                return [];
            }

            if (spellComponent.IsOnCooldown()) {
                Logger.Combat.Debug($"Spell '{pSource.DisplayName}' is still on cooldown");
                return [];
            }

            spellComponent.Cast();
            CompleteChainCast(pSource, pChainSpellComponent);
            return spellComponent.Effects;
        }

        return pChainSpellComponent.Effects;
    }

    private static void CompleteChainCast(Gizmo pSource, ChainSpellComponent pChainSpellComponent) {
        Gizmo nextSpell = pChainSpellComponent.GetNextSpell() ?? pSource;
        pChainSpellComponent.Chain();
        ulong cooldownInMicroSeconds = nextSpell.GetCooldown() * 1_000_000;
        pSource.EmitCasted(cooldownInMicroSeconds);
    }

    private void OnDamageEffectTick(DamageEffectComponent pDamageEffect, CombatManager pSource, int pCoefficient) {
        double damage = CombatSystem.CalculateDamage(pDamageEffect, pSource.CombatSystem);
        
        damage *= _appliedStacks[pDamageEffect] * pCoefficient;
        
        _combatResources.ModifyHealth(damage);
    }

    private void OnStatEffectTick(StatEffectComponent pEffect, CombatManager pSource, int pCoefficient) { }

    private void LinkEntityArmory() {
        foreach (GizmoStack gizmoStack in GetEntity().Armory.Gizmos) {
            LinkGizmoAttributes(gizmoStack, -1);
        }
    }

    private void LinkGizmoAttributes(GizmoStack pGizmoStack, int pIndex) {
        var component = pGizmoStack.Gizmo?.GetComponent<StatComponent>();

        if (component is null) {
            return;
        }

        _statSystem.Link(component.Attributes);
    }

    private void UnlinkGizmoAttributes(GizmoStack pGizmoStack, int pIndex) {
        var component = pGizmoStack.Gizmo?.GetComponent<StatComponent>();

        if (component is null) {
            return;
        }

        _statSystem.Unlink(component.Attributes);
    }

    private Entity GetEntity() {
        return GetParent<Entity>();
    }
}