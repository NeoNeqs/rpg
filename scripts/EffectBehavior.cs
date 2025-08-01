using Godot;
using RPG.scripts.effects;
using RPG.scripts.inventory;
using RPG.world.entity;

namespace RPG.scripts;


[GlobalClass]
public abstract partial class EffectBehavior : Resource {
    public abstract void Run(Entity pSourceEntity, Gizmo pEffectOwner, Effect pEffect, Entity pTargetEntity);
}