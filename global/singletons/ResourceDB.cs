using System.Collections.Generic;
using Godot;
using RPG.global.enums;
using RPG.global.tools;
using RPG.scripts;
using RPG.scripts.effects;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;
using Faction = RPG.scripts.Faction;

namespace RPG.global.singletons;

[Tool]
public sealed partial class ResourceDB : Node {
    private const string ResourceRoot = "res://resources/";
    private const string ItemsLocation = $"{ResourceRoot}items/";
    private const string SpellsLocation = $"{ResourceRoot}spells/";
    private const string EffectsLocation = $"{ResourceRoot}effects/";
    private const string FactionsLocation = $"{ResourceRoot}effects/";

    private const string SpellPrefix = "spell:";
    private const string ItemPrefix = "item:";
    private const string EffectPrefix = "effect:";
    private const string FactionPrefix = "faction:";

    public static string ItemIdsHintString = string.Empty;
    public static string SpellIdsHintString = string.Empty;
    public static string EffectIdsHintString = string.Empty;
    public static string FactionIdsHintString = string.Empty;

    public static Effect? GetEffect(string pId) {
        return GetT<Effect>(pId, EffectPrefix, EffectsLocation);
    }

    public static Gizmo? GetItem(string pId) {
        return GetT<Gizmo>(pId, ItemPrefix, ItemsLocation);
    }

    public static Gizmo? GetSpell(string pId) {
        return GetT<Gizmo>(pId, SpellPrefix, SpellsLocation);
    }

    public static Faction? GetFaction(string pId) {
        return GetT<Faction>(pId, FactionPrefix, FactionsLocation);
    }

    private static T? GetT<T>(string pId, string pPrefix, string pLocation) where T : Resource {
#if TOOLS
        if (!pId.StartsWith(pPrefix)) {
            Logger.Core.Error($"Id '{pId}' does not specify a prefix for the requested resource.", true);
            return null;
        }
#endif

        string resourcePath = pLocation.PathJoin(pId.TrimPrefix(pPrefix).Replace(':', '/')) + ".tres";
        return GD.Load<T>(resourcePath);
    }

#if TOOLS
    private static ResourceDB _instance = null!;
    private const int MaxRecursionDepth = 2;


    private struct PathData {
        private readonly string _basePath;
        public string CurrentPath;

        public PathData(string pBasePath) {
            if (!pBasePath.EndsWith('/')) {
                pBasePath += "/";
            }

            _basePath = pBasePath;
            CurrentPath = pBasePath;
        }

        public PathData Branch(string pNewPath) {
            return new PathData(_basePath) { CurrentPath = pNewPath };
        }

        public string GetSubDir() {
            return CurrentPath[_basePath.Length..];
        }
    }

    public ResourceDB() {
        _instance = this;
    }

    public override void _EnterTree() {
        // IMPORTANT: Loading will not function properly during construction of this object. 
        //            Editor will complain about not being able to cast a resource to Effect even though the resource
        //            is clearly an Effect...
        //            This does not happen to Gizmos, which makes me think it's due to Effect having an inheritance 
        //            chain of StackingEffect, DamageEffect, etc. and those classes are not registered at the time
        //            Loading in _EnterTree solves the problem *shrug*.
        Load();
    }

    private static void Load() {
        if (!Engine.IsEditorHint()) {
            return;
        }

        Logger.Core.Info("Loading Resource Database.");

        Dictionary<Id, string> items = LoadRecursive(ItemsLocation, ItemPrefix, MaxRecursionDepth);
        Dictionary<Id, string> spells = LoadRecursive(SpellsLocation, SpellPrefix, MaxRecursionDepth);
        Dictionary<Id, string> effects = LoadRecursive(EffectsLocation, EffectPrefix, MaxRecursionDepth);
        Dictionary<Id, string> factions = LoadRecursive(FactionsLocation, FactionPrefix, MaxRecursionDepth);

        ItemIdsHintString = string.Join(',', items.Keys);
        SpellIdsHintString = string.Join(',', spells.Keys);
        EffectIdsHintString = string.Join(',', effects.Keys);
        FactionIdsHintString = string.Join(',', factions.Keys);

        Logger.Core.Info("Done: Loading Resource Database.");

        // IMPORTANT: Validation must happen during editor hint, otherwise ResourceSaver will not save resources correctly.

        Logger.Core.Info("Validating Resource Database.");

        ValidateGizmos(items, ItemsLocation);
        ValidateGizmos(spells, SpellsLocation);
        ValidateEffects(effects, EffectsLocation);
        ValidateFactions(factions, FactionsLocation);

        Logger.Core.Info("Done: Validating Resource Database.");
    }

    private static Dictionary<Id, string> LoadRecursive(string pFrom, string pPrefix, int pDepth) {
        using var clock = new Clock(_instance, $"\tLoading {pFrom}", true);

        Dictionary<Id, string> resources = [];

        LoadRecursive(new PathData(pFrom), pPrefix, pDepth, resources);

        return resources;
    }

    private static void LoadRecursive(PathData pData, string pPrefix, int pDepth, Dictionary<Id, string> pResources) {
        if (pDepth < 0) {
            Logger.Core.Error($"Can't load Resources from '{pData.CurrentPath:url}'. Max recursion depth reached!");
            return;
        }

        string[] contents = ResourceLoader.ListDirectory(pData.CurrentPath);

        if (contents.Length == 0) {
            Logger.Core.Error(
                $"Error listing contents of '{pData.CurrentPath:url}'. Directory is either empty or does not exist!",
                true);
            return;
        }

        foreach (string item in contents) {
            string fullPath = pData.CurrentPath.PathJoin(item);

            // Directory:
            if (item.EndsWith('/')) {
                LoadRecursive(pData.Branch(fullPath), pPrefix, pDepth - 1, pResources);
                continue;
            }

            // File:
            Id id = FilePathToId(pPrefix, pData.GetSubDir(), item.GetBaseName());

            if (pResources.TryGetValue(id, out string? existingPath)) {
                Logger.Core.Error(
                    $"Cannot load resource '{fullPath:url}'. A resource with the same id already exists at '{existingPath:url}'.");
                continue;
            }

            pResources[id] = fullPath;
        }
    }

    private static Id FilePathToId(string pPrefix, string pSubDir, string pFileBaseName) {
        pSubDir = pSubDir.Replace('/', ':');

        return new Id(pPrefix + pSubDir + pFileBaseName);
    }

    private static void ValidateGizmos(Dictionary<Id, string> pGizmos, string pFrom) {
        using var clock = new Clock(_instance, $"\tValidating {pFrom}", true);

        foreach ((Id key, string fullPath) in pGizmos) {
            var gizmo = ResourceLoader.Load<Gizmo?>(fullPath);

            if (gizmo is null) {
                Logger.Core.Error($"Cannot not load Resource '{fullPath:url}'. It may not be a valid {nameof(Gizmo)}.");
                continue;
            }

            bool changed = ValidateId(gizmo, key);
            CheckGizmoComponents(gizmo, pGizmos);

            if (gizmo.DisplayName.Length == 0) {
                Logger.Core.Warn($"Gizmo '{fullPath:url}' does not have a display name set!");
            }

            if (gizmo.Icon is null) {
                Logger.Core.Warn($"Gizmo '{fullPath:url}' is missing an icon!");
            }

            if (changed) {
                Error error = ResourceSaver.Save(gizmo, fullPath);
                if (error != Error.Ok) {
                    Logger.Core.Error($"Could not save {nameof(Gizmo)} '{fullPath:url}'. Error code: {error:G}.");
                }
            }
        }
    }

    private static void ValidateEffects(Dictionary<Id, string> pEffects, string pFrom) {
        using var clock = new Clock(_instance, $"\tValidating {pFrom}", true);

        foreach ((Id id, string fullPath) in pEffects) {
            var effect = ResourceLoader.Load<Effect?>(fullPath);

            if (effect is null) {
                Logger.Core.Error($"Could not load Resource '{fullPath:url}'. It may not be a valid {nameof(Effect)}.");
                continue;
            }

            bool changed = ValidateId(effect, id);
            CheckExcludingEffects(effect, pEffects);

            if (effect.DisplayName.Length == 0) {
                Logger.Core.Warn($"Gizmo '{fullPath:url}' does not have a display name set!");
            }

            if (effect.Icon is null) {
                Logger.Core.Warn($"Gizmo '{fullPath:url}' is missing an icon!");
            }

            if (changed) {
                Error error = ResourceSaver.Save(effect, fullPath);
                if (error != Error.Ok) {
                    Logger.Core.Error($"Could not save {nameof(Effect)} '{fullPath:url}'. Error code: {error:G}.");
                }
            }
        }
    }

    private static void ValidateFactions(Dictionary<Id, string> pFactions, string pFrom) {
        using var clock = new Clock(_instance, $"\tValidating {pFrom}", true);


        foreach ((Id id, string fullPath) in pFactions) {
            var faction = ResourceLoader.Load<Faction?>(fullPath);

            if (faction is null) {
                Logger.Core.Error(
                    $"Could not load Resource '{fullPath:url}'. It may not be a valid {nameof(Faction)}.");
                continue;
            }

            if (faction.DisplayName.Length == 0) {
                Logger.Core.Warn($"Faction '{fullPath:url}' does not have a display name set!");
            }

            bool changed = ValidateId(faction, id);

            if (changed) {
                Error error = ResourceSaver.Save(faction, fullPath);
                if (error != Error.Ok) {
                    Logger.Core.Error($"Could not save {nameof(Faction)} '{fullPath:url}'. Error code: {error:G}.");
                }
            }
        }
    }

    private static bool ValidateId<T>(T pNamed, Id pId) where T : Resource, INamedIdentifiable {
        bool changed = false;

        if (pNamed.Id != pId) {
            pNamed.SetProperty(nameof(Gizmo.Id), pId);
            changed = true;
        }

        CheckId(pNamed);

        return changed;
    }

    private static void CheckId<T>(T pNamed) where T : Resource, INamedIdentifiable {
        bool detectedUpperCase = false;
        bool detectedNonPrintableASCII = false;

        string id = pNamed.Id.ToString();
        foreach (char c in id) {
            if (char.IsUpper(c)) {
                detectedUpperCase = true;
            }

            if (char.IsControl(c) || char.IsWhiteSpace(c) || !char.IsAscii(c)) {
                detectedNonPrintableASCII = true;
            }
        }

        if (detectedUpperCase) {
            Logger.Core.Warn($"Id '{id}' of '{pNamed.ResourcePath:url}' should not contain upper case letters!");
        }

        if (detectedNonPrintableASCII) {
            Logger.Core.Warn($"Id '{id}' of '{pNamed.ResourcePath:url}' must contain only printable ASCII characters!");
        }
    }

    private static void CheckGizmoComponents(Gizmo pGizmo, Dictionary<Id, string> pSpells) {
        bool foundSpellComponent = false;
        bool foundSequenceSpellComponent = false;

        foreach ((string _, GizmoComponent? value) in pGizmo.Components) {
            switch (value) {
                case SequenceSpellComponent chainSpellComponent:
                    foundSequenceSpellComponent = true;

                    CheckSpellEffects(pGizmo, chainSpellComponent);
                    CheckSpellsOfSequenceSpell(pGizmo, chainSpellComponent);
                    CheckLinkedSpells(pGizmo, chainSpellComponent.LinkedSpells, pSpells);

                    break;
                case SpellComponent spellComponent:
                    foundSpellComponent = true;

                    CheckSpellEffects(pGizmo, spellComponent);
                    CheckLinkedSpells(pGizmo, spellComponent.LinkedSpells, pSpells);

                    break;
                case null:
                    Logger.Core.Warn(
                        $"{nameof(Gizmo)} '{pGizmo.ResourcePath:url}' has a null {nameof(GizmoComponent)}");
                    break;
            }
        }

        // Sanity check: since SequenceSpellComponent and SpellComponent is weird, I need to warn myself to avoid making mistakes.
        // SequenceSpellComponent will take priority over SpellComponent!
        if (foundSpellComponent && foundSequenceSpellComponent) {
            Logger.Core.Warn(
                $"{nameof(Gizmo)} '{pGizmo.ResourcePath:url}' should not contain both {nameof(SpellComponent)} and {nameof(SequenceSpellComponent)}.");
        }
    }

    private static void CheckSpellsOfSequenceSpell(Gizmo pGizmo, SequenceSpellComponent pSequenceSpellComponent) {
        foreach (Gizmo spell in pSequenceSpellComponent.Spells) {
            if (spell.ResourcePath.Contains("::")) {
                Logger.Core.Warn(
                    $"{nameof(SequenceSpellComponent)} in {nameof(Gizmo)} '{pGizmo.ResourcePath:url}' should not have built-in Spell(s). Instead save them to a file at '{SpellsLocation}'.");
                break;
            }
        }
    }

    private static void CheckLinkedSpells(Gizmo pGizmo, Id[] pLinkedSpells, Dictionary<Id, string> pSpells) {
        foreach (Id linkedSpell in pLinkedSpells) {
            if (!linkedSpell.ToString().StartsWith(SpellPrefix)) {
                Logger.Core.Error(
                    $"A {nameof(SpellComponent)} of '{pGizmo.ResourcePath:url}', is linking a spell with id '{linkedSpell}' that does not start with '{SpellPrefix}' prefix.");
                continue;
            }

            if (!pSpells.ContainsKey(linkedSpell)) {
                Logger.Core.Error(
                    $"A {nameof(SpellComponent)} of '{pGizmo.ResourcePath:url}' is linking a spell with unknown id '{linkedSpell}'.");
            }
        }
    }

    private static void CheckSpellEffects(Gizmo pGizmo, SpellComponent pSpellComponent) {
        foreach (Effect? effect in (Effect?[])pSpellComponent.GetEffects()) {
            if (effect is null) {
                Logger.Core.Warn($"{nameof(Gizmo)} '{pGizmo.ResourcePath:url}' has a null {nameof(Effect)}.");
                continue;
            }

            if (effect.ResourcePath.Contains("::")) {
                Logger.Core.Warn(
                    $"{nameof(Gizmo)} '{pGizmo.ResourcePath:url}' should not have built-in {nameof(Effect)}(s). Instead save them to a file at '{EffectsLocation}'.");
                break;
            }


            // TODO: Check if SetProperty works
            if (effect.Behavior is AoeEffectBehavior) {
                var currentSpellFlags = pSpellComponent.GetProperty<SpellFlags>(nameof(SpellComponent.SpellFlags));

                pSpellComponent.SetProperty(nameof(SpellComponent.SpellFlags),
                    currentSpellFlags | SpellFlags.InternalIsAoe);
            }
        }
    }

    private static void CheckExcludingEffects(Effect pEffect, Dictionary<Id, string> pEffects) {
        foreach (Id id in pEffect.ExcludingEffects) {
            string idString = id.ToString();
            if (!idString.StartsWith(EffectPrefix)) {
                Logger.Core.Error(
                    $"In {nameof(Effect)} '{pEffect.ResourcePath:url}', excluded id '{id}' is not a valid effect id.");
            } else if (!pEffects.ContainsKey(idString)) {
                Logger.Core.Error(
                    $"{nameof(Effect)} '{pEffect.ResourcePath:url}' is excluding an effect with unknown id '{id}'.");
            }
        }
    }

#endif
}