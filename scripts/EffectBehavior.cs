using Godot;
using RPG.scripts.combat;
using RPG.world.entity;

namespace RPG.scripts;

[GlobalClass]
public abstract partial class EffectBehavior : Resource {
    public abstract void Run(Entity pSource, AppliedEffect pAppliedEffect, Entity pTarget, bool pIsHarmful);
}