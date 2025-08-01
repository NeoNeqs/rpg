using Godot;
using RPG.scripts.combat;
using RPG.scripts.inventory;
using RPG.ui.elements;
using RPG.world.character;
using RPG.world.entity;

namespace RPG.global.singletons;

public sealed partial class EventBus : Node {
    [Signal]
    public delegate void CharacterInventoryLoadedEventHandler(Inventory pInventory);

    [Signal]
    public delegate void CharacterSpellBookLoadedEventHandler(Inventory pInventory);

    [Signal]
    public delegate void CharacterStatsLoadedEventHandler(Stats pStats);

    /// <summary>
    ///     Emitted when the player clicks on an empty <see cref="RPG.ui.UI" /> region where there are no <see cref="UIElement" />.
    /// </summary>
    [Signal]
    public delegate void EmptyRegionPressedEventHandler();

    /// <summary>
    ///     Emitted when the player selects an <see cref="Entity" /> with a mouse button.
    /// </summary>
    [Signal]
    public delegate void EntitySelectedEventHandler(Entity pEntity);

    [Signal]
    public delegate void HotbarKeyPressedEventHandler(Gizmo pGizmo);

    /// <summary>
    ///     Emitted when <see cref="PlayerCharacter" />'s current target changed
    /// </summary>
    [Signal]
    public delegate void PlayerTargetChangedEventHandler(PlayerCharacter pPlayerCharacter, Entity? pNewTarget);

    /// <summary>
    ///     Emitted when the global <see cref="Stats" /> are loaded.
    /// </summary>
    [Signal]
    public delegate void TotalStatsLoadedEventHandler(Stats pStats);

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

    public void EmitEntitySelectedEventHandler(Entity pEntity) {
        EmitSignalEntitySelected(pEntity);
    }

    public void EmitPlayerTargetChanged(PlayerCharacter pPlayerCharacter, Entity? pNewTarget) {
        EmitSignalPlayerTargetChanged(pPlayerCharacter, pNewTarget);
    }
}