using System.Diagnostics;
using System.Reflection;
using Godot;
using Godot.NativeInterop;
using RPG.scripts.components;
using RPG.scripts.effects;
using RPG.scripts.inventory;
using RPG.world;

namespace RPG.scripts.combat;

public partial class CombatManager : Node {
    private Entity? _selectedEntity;

    [Export] public StatSystem StatSystem;
    [Export] public AttributeSystem AttributeSystem;

    public override void _Ready() {
        Entity attacker = GetAttacker();
        
        Debug.Assert(GetAttacker() is not null);

        attacker.Armory.GizmoChanged += LinkGizmoAttributes;
        attacker.Armory.GizmoAboutToChange += UnlinkGizmoAttributes;
        
        AttributeSystem.Link(attacker.BaseAttributes);
        LinkAttackerArmory();
        
        StatSystem.Initialize(AttributeSystem.Total);
    }

    public void ApplyEffectToSelectedEntity(Effect pEffect) {
        _selectedEntity?.CombatManager.ApplyEffect(pEffect, this);
    }

    public void ApplyEffect(Effect pEffect, CombatManager pAttacker) {
        Timer? timer = pEffect.Setup();
    
        if (timer is not null) {
            AddChild(timer);
        }
    
        switch (pEffect) {
            case DamageEffect damageEffect:
                damageEffect.Tick += (DamageEffect pDamageEffect) => {
                    OnDamageEffectTick(pDamageEffect, pAttacker);
                };
                
                break;
            case AttributeEffect attributeEffect:
                attributeEffect.Tick += OnAttributeEffectTick;
                break;
        }
    }

    private void OnAttributeEffectTick(AttributeEffect pEffect) {
        
    }

    private void OnDamageEffectTick(DamageEffect pEffect, CombatManager pAttacker) {
        StatSystem.CalculateDamage(pEffect, pAttacker.StatSystem);
    }

    private void LinkAttackerArmory() {
        foreach (GizmoStack gizmoStack in GetAttacker().Armory.Gizmos) {
            LinkGizmoAttributes(gizmoStack);
        }
    }

    private void LinkGizmoAttributes(GizmoStack pGizmoStack) {
        if (pGizmoStack.Gizmo is null) {
            return;
        }

        var component = pGizmoStack.Gizmo.GetComponent<AttributeComponent>();

        if (component is null) {
            return;
        }
        
        AttributeSystem.Link(component.Attributes);
    }

    private void UnlinkGizmoAttributes(GizmoStack pGizmoStack) {
        if (pGizmoStack.Gizmo is null) {
            return;
        }

        var component = pGizmoStack.Gizmo.GetComponent<AttributeComponent>();

        if (component is null) {
            return;
        }
        
        AttributeSystem.Unlink(component.Attributes);
    }

    private Entity GetAttacker() {
        return GetParent<Entity>();
    }
}