using Godot;

namespace RPG.global;

public static class AssetDB {
    public static readonly PackedScene DummyEntity = GD.Load<PackedScene>("res://world/dummy_entity.tscn");
}