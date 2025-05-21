using Godot;
using RPG.scripts.combat;
using RPG.scripts.inventory;

namespace RPG.global;

public partial class EventBus : Node {
    [Signal]
    public delegate void EmptyRegionPressedEventHandler();
    
    [Signal]
    public delegate void CharacterInventoryLoadedEventHandler(Inventory pInventory);
    
    [Signal]
    public delegate void CharacterAttributesLoadedEventHandler(Attributes pAttributes);
    
    [Signal]
    public delegate void TotalAttributesLoadedEventHandler(Attributes pAttributes);
    

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

    public void EmitCharacterAttributesLoaded(Attributes pAttributes) {
        EmitSignalCharacterAttributesLoaded(pAttributes);
    }

    public void EmitTotalAttributesLoaded(Attributes pAttributes) {
        EmitSignalTotalAttributesLoaded(pAttributes);
    }
}