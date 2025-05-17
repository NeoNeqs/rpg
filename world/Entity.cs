using Godot;
using RPG.scripts.combat;
using RPG.scripts.inventory;

namespace RPG.world;

public partial class Entity : Node3D {

    [Export] public CombatManager CombatManager;
    [Export] public Inventory Armory;
    [Export] public Attributes BaseAttributes;


}