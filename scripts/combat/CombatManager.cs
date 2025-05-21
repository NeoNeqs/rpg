using System.Diagnostics;
using Godot;
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

    [Export] public CombatSystem CombatSystem = null!;
    [Export] public StatSystem StatSystem = null!;
    [Export] public CombatResources CombatResources = null!;

    public override void _Ready() {
        Entity entity = GetEntity();

        Debug.Assert(entity is not null);

        entity.Armory.GizmoChanged += LinkGizmoAttributes;
        entity.Armory.GizmoAboutToChange += UnlinkGizmoAttributes;

        StatSystem.Link(entity.BaseStats);
        LinkEntityArmory();

        CombatSystem.Initialize(StatSystem.Total);
        CombatResources.Initialize(CombatSystem);
    }

    public void Cast(Gizmo pGizmo) {
        ChainSpellComponent? chainSpellComponent = pGizmo.GetComponent<ChainSpellComponent>();

        if (chainSpellComponent is not null) {
            Effect[] effects = CastChainSpell(pGizmo, chainSpellComponent);
            ApplyEffectsToTarget(effects);
        }

        SpellComponent? spellComponent = pGizmo.GetComponent<SpellComponent>();

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
        if (!HasTarget()) {
            return;
        }

        AreaOfEffectComponent? areaEffectComponent = pEffect.GetComponent<AreaOfEffectComponent>();
        if (areaEffectComponent is not null) {
            foreach (Entity entityInRadius in TargetEntity!.GetEntitiesInRadius(areaEffectComponent.Radius)) {
                entityInRadius.CombatManager.ApplyEffect(pEffect, this);
            }
        }

        TargetEntity!.CombatManager.ApplyEffect(pEffect, this);
    }

    private void ApplyEffect(Effect pEffect, CombatManager pAttacker) {
        Timer? timer = pEffect.Start();

        if (timer is not null) {
            AddChild(timer);
        }

        DamageEffectComponent? damageEffectComponent = pEffect.GetComponent<DamageEffectComponent>();
        if (damageEffectComponent is not null) {
            pEffect.OnTick += () => OnDamageEffectTick(damageEffectComponent, pAttacker);
        }

        StatEffectComponent? attributeEffectComponent = pEffect.GetComponent<StatEffectComponent>();
        if (attributeEffectComponent is not null) {
            pEffect.OnTick += () => OnAttributeEffectTick(attributeEffectComponent, pAttacker);
        }
    }

    private bool HasTarget() {
        return TargetEntity != null && IsInstanceValid(TargetEntity);
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
            SpellComponent? spellComponent = currentSpell.GetComponent<SpellComponent>();
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

    private void OnDamageEffectTick(DamageEffectComponent pDamageEffect, CombatManager pAttacker) {
        // TODO: Cache the damage value, since it won't probably change.
        double damage = CombatSystem.CalculateDamage(pDamageEffect, pAttacker.CombatSystem);

        CombatResources.ApplyDamage(damage);
    }

    private void OnAttributeEffectTick(StatEffectComponent pEffect, CombatManager pAttacker) { }


    private void LinkEntityArmory() {
        foreach (GizmoStack gizmoStack in GetEntity().Armory.Gizmos) {
            LinkGizmoAttributes(gizmoStack, -1);
        }
    }

    private void LinkGizmoAttributes(GizmoStack pGizmoStack, int pIndex) {
        StatComponent? component = pGizmoStack.Gizmo?.GetComponent<StatComponent>();

        if (component is null) {
            return;
        }

        StatSystem.Link(component.Attributes);
    }

    private void UnlinkGizmoAttributes(GizmoStack pGizmoStack, int pIndex) {
        StatComponent? component = pGizmoStack.Gizmo?.GetComponent<StatComponent>();

        if (component is null) {
            return;
        }

        StatSystem.Unlink(component.Attributes);
    }

    private Entity GetEntity() {
        return GetParent<Entity>();
    }
}