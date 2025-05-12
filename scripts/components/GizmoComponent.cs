using Godot;

namespace RPG.scripts.components;

public abstract partial class GizmoComponent : Resource {

    public virtual bool IsAllowed(GizmoComponent pOtherComponent) {
        return true;
    }

}