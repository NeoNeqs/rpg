using System.Diagnostics;
using Godot;

namespace RPG.global;


public static class AssetDB {
    public static readonly PackedScene DummyEntity = GD.Load<PackedScene>("uid://cwqk02m3lsujx");
    public static readonly PackedScene ItemView = GD.Load<PackedScene>("uid://b0h7oudoccqxk");
    public static readonly PackedScene SpellView = GD.Load<PackedScene>("uid://d4bn283eravlm");
    public static readonly PackedScene HotbarView = GD.Load<PackedScene>("uid://dpcqcelyap0p1");

    public static readonly PackedScene StatView = GD.Load<PackedScene>("uid://bgftddgnyd2kb");

#if TOOLS
    static AssetDB() {
        Debug.Assert(DummyEntity is not null);
        Debug.Assert(ItemView is not null);
        Debug.Assert(SpellView is not null);
        Debug.Assert(HotbarView is not null);
        Debug.Assert(StatView is not null);
    }
#endif
}