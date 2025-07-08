using Godot;
using RPG.world;
using Entity = RPG.world.entity.Entity;

namespace RPG.scripts.inventory.components;

/// <summary>
/// <para>Base class for Gizmo components.</para>
/// <para><b>Note:</b> All GizmoComponents will be duplicated if they're placed in an <see cref="Inventory"/></para> 
/// </summary>
[Tool, GlobalClass]
public abstract partial class GizmoComponent : Resource {

    public virtual bool IsAllowed(GizmoComponent pOtherComponent) {
        return true;
    }

    public virtual string GetTooltip(Entity pOwner) {
        return "";
    }
}