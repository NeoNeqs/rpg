using Godot;
using Godot.Collections;
using RPG.scripts.combat;
using RPG.scripts.effects;
using RPG.scripts.effects.components;
using RPG.scripts.inventory;
using RPG.world;

namespace RPG.global;

public partial class EventBus : Node {
    [Signal]
    public delegate void EmptyRegionPressedEventHandler();
    
    [Signal]
    public delegate void CharacterInventoryLoadedEventHandler(Inventory pInventory);
    
    [Signal]
    public delegate void CharacterStatsLoadedEventHandler(Stats pStats);
    
    [Signal]
    public delegate void CharacterSpellBookLoadedEventHandler(Inventory pInventory);
    
    [Signal]
    public delegate void TotalStatsLoadedEventHandler(Stats pStats);
    
    [Signal]
    public delegate void HotbarKeyPressedEventHandler(Gizmo pGizmo);

    [Signal]
    public delegate void AoESelectedEventHandler(Array<Effect> pAoEs);
    
    [Signal]
    public delegate void EntitySelectedEventHandler(Entity pEntity);
    

    public static EventBus Instance = null!;

    public EventBus() {
        Instance = this;
    }

    public void EmitEmptyRegionPressed() {
        EmitSignalEmptyRegionPressed();
    }

    public void EmitCharacterInventoryLoaded(Inventory pInventory) {
        EmitSignalCharacterInventoryLoaded(pInventory);
    }

    public void EmitCharacterStatsLoaded(Stats pStats) {
        EmitSignalCharacterStatsLoaded(pStats);
    }

    public void EmitCharacterSpellBookLoaded(Inventory pInventory) {
        EmitSignalCharacterSpellBookLoaded(pInventory);
    }
    public void EmitTotalStatsLoaded(Stats pStats) {
        EmitSignalTotalStatsLoaded(pStats);
    }

    public void EmitHotbarKeyPressed(Gizmo pGizmo) {
        EmitSignalHotbarKeyPressed(pGizmo);
    }

    public void EmitAoESelected(Array<Effect> pAoEs) {
        EmitSignalAoESelected(pAoEs);
    }

    public void EmitEntitySelectedEventHandler(Entity pEntity) {
        EmitSignalEntitySelected(pEntity);
    }
}