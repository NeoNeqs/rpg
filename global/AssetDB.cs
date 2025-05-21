using Godot;

namespace RPG.global;

public static class AssetDB {
    public static readonly PackedScene DummyEntity = GD.Load<PackedScene>("uid://cwqk02m3lsujx");
    public static readonly PackedScene ItemView = GD.Load<PackedScene>("uid://b0h7oudoccqxk");
    public static readonly PackedScene SpellView = GD.Load<PackedScene>("uid://d4bn283eravlm");
    public static readonly PackedScene HotbarView = GD.Load<PackedScene>("uid://dpcqcelyap0p1");

    public static readonly PackedScene StatView = GD.Load<PackedScene>("uid://bgftddgnyd2kb");

}