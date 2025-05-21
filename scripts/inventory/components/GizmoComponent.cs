using Godot;
using RPG.world;

namespace RPG.scripts.inventory.components;

public abstract partial class GizmoComponent : Resource {

    public virtual bool IsAllowed(GizmoComponent pOtherComponent) {
        return true;
    }

    public virtual string GetTooltip(Entity pOwner) {
        return "";
    }
}