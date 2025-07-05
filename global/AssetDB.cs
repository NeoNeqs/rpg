using System.Diagnostics;
using Godot;
using RPG.global.tools;

namespace RPG.global;


public static class AssetDB {
    public static readonly PackedScene DummyEntity = GD.Load<PackedScene>("uid://cwqk02m3lsujx");
    
    public static readonly PackedScene ItemView = GD.Load<PackedScene>("uid://b0h7oudoccqxk");
    public static readonly PackedScene SpellView = GD.Load<PackedScene>("uid://d4bn283eravlm");
    public static readonly PackedScene HotbarView = GD.Load<PackedScene>("uid://dpcqcelyap0p1");
    public static readonly PackedScene StatView = GD.Load<PackedScene>("uid://bgftddgnyd2kb");

#if TOOLS
    static AssetDB() {
        Tools.Assert(DummyEntity is not null, "AssetDB.DummyEntity is null. Verify that the uid is correct.");
        Tools.Assert(ItemView is not null, "AssetDB.ItemView is null. Verify that the uid is correct.");
        Tools.Assert(SpellView is not null, "AssetDB.SpellView is null. Verify that the uid is correct.");
        Tools.Assert(HotbarView is not null, "AssetDB.HotbarView is null. Verify that the uid is correct.");
        Tools.Assert(StatView is not null, "AssetDB.StatView is null. Verify that the uid is correct.");
    }
#endif
}