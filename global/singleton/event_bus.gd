extends Node

# TODO: change player to character
@warning_ignore("unused_signal")
signal player_inventory_loaded(p_inventory: Inventory)


@warning_ignore("unused_signal")
signal hotbar_key_pressed(p_spell: Item)


@warning_ignore("unused_signal")
signal character_attributes_changed(p_attributes: Attributes)
