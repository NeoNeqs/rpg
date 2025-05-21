using System.Diagnostics;
using Godot;
using RPG.Global;
using RPG.scripts.effects;
using RPG.scripts.effects.components;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;
using RPG.world;

namespace RPG.scripts.combat;

public partial class CombatManager : Node {
    public Entity? TargetEntity;

    [Export] public StatSystem StatSystem = null!;
    [Export] public AttributeSystem AttributeSystem = null!;
    [Export] public CombatResources CombatResources = null!;

    public override void _Ready() {
        Entity entity = GetEntity();

        Debug.Assert(entity is not null);

        entity.Armory.GizmoChanged += LinkGizmoAttributes;
        entity.Armory.GizmoAboutToChange += UnlinkGizmoAttributes;

        AttributeSystem.Link(entity.BaseAttributes);
        LinkEntityArmory();

        StatSystem.Initialize(AttributeSystem.Total);
        CombatResources.Initialize(StatSystem);
    }

    public void Cast(Gizmo pGizmo) {
        var chainSpellComponent = pGizmo.GetComponent<ChainSpellComponent>();

        if (chainSpellComponent is not null) {
            Effect[] effects = CastChainSpell(pGizmo, chainSpellComponent);
            ApplyEffectsToTarget(effects);
        }

        var spellComponent = pGizmo.GetComponent<SpellComponent>();

        // ReSharper disable once InvertIf
        if (spellComponent is not null) {
            Effect[] effects = CastSpell(pGizmo, spellComponent);
            ApplyEffectsToTarget(effects);
        }
    }

    public void ApplyEffectToTargetEntity(Effect pEffect) {
        if (TargetEntity is null) {
            return;
        }

        var areaEffectComponent = pEffect.GetComponent<AreaEffectComponent>();
        if (areaEffectComponent is not null) {
            foreach (Entity entityInRadius in TargetEntity.GetEntitiesInRadius(areaEffectComponent.Radius)) {
                entityInRadius.CombatManager.ApplyEffect(pEffect, this);
            }
        }

        TargetEntity.CombatManager.ApplyEffect(pEffect, this);
    }
    
    private void ApplyEffectsToTarget(Effect[] pEffects) {
        foreach (Effect effect in pEffects) {
            ApplyEffectToTargetEntity(effect);
        }
    }


    public void ApplyEffect(Effect pEffect, CombatManager pAttacker) {
        Timer? timer = pEffect.Start();

        if (timer is not null) {
            AddChild(timer);
        }

        var damageEffectComponent = pEffect.GetComponent<DamageEffectComponent>();
        if (damageEffectComponent is not null) {
            pEffect.OnTick += () => OnDamageEffectTick(damageEffectComponent, pAttacker);
        }

        var attributeEffectComponent = pEffect.GetComponent<AttributeEffectComponent>();
        if (attributeEffectComponent is not null) {
            pEffect.OnTick += () => OnAttributeEffectTick(attributeEffectComponent, pAttacker);
        }
    }

    public bool HasTarget() {
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

    private void OnDamageEffectTick(DamageEffectComponent pDamageEffect, CombatManager pAttacker) {
        // TODO: Cache the damage value, since it won't probably change.
        double damage = StatSystem.CalculateDamage(pDamageEffect, pAttacker.StatSystem);

        CombatResources.ApplyDamage(damage);
    }

    private void OnAttributeEffectTick(AttributeEffectComponent pEffect, CombatManager pAttacker) { }


    private void LinkEntityArmory() {
        foreach (GizmoStack gizmoStack in GetEntity().Armory.Gizmos) {
            LinkGizmoAttributes(gizmoStack, -1);
        }
    }

    private void LinkGizmoAttributes(GizmoStack pGizmoStack, int pIndex) {
        AttributeComponent? component = pGizmoStack.Gizmo?.GetComponent<AttributeComponent>();

        if (component is null) {
            return;
        }

        AttributeSystem.Link(component.Attributes);
    }

    private void UnlinkGizmoAttributes(GizmoStack pGizmoStack, int pIndex) {
        AttributeComponent? component = pGizmoStack.Gizmo?.GetComponent<AttributeComponent>();

        if (component is null) {
            return;
        }

        AttributeSystem.Unlink(component.Attributes);
    }

    private Entity GetEntity() {
        return GetParent<Entity>();
    }
}