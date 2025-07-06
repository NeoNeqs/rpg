using System.Collections.Generic;
using Godot;
using RPG.global.tools;
using RPG.scripts.effects;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;
using Id = Godot.StringName;

namespace RPG.global.singletons;

[Tool]
public sealed partial class ResourceDB : Node {
    private static readonly Dictionary<Id, string> Resources = [];

    private static ResourceDB _instance = null!;

    private const int MaxRecursionDepth = 2;

    private const string ItemLocation = "res://resources/items/";
    private const string SpellLocation = "res://resources/spells/";
    private const string EffectLocation = "res://resources/effects/";

    private const string SpellPrefix = "spell:";
    private const string ItemPrefix = "item:";
    private const string EffectPrefix = "effect:";

    public ResourceDB() {
        _instance = this;
    }

    public override void _EnterTree() {
        LoadResources();
    }

    public static Effect? GetEffect(string pId) {
        return GetT<Effect>(pId, EffectPrefix);
    }

    public static Gizmo? GetItem(string pId) {
        return GetT<Gizmo>(pId, ItemPrefix);
    }

    public static Gizmo? GetSpell(string pId) {
        return GetT<Gizmo>(pId, SpellPrefix);
    }

    private static T? GetT<T>(string pId, string pPrefix) where T : Resource {
#if TOOLS
        if (!pId.StartsWith(pPrefix)) {
            Logger.Core.Error(
                $"Id '{pId}' does not specify a prefix for the requested resource. Falling back to {pPrefix}{pId}.",
                true);
            return null;
        }
#endif

        if (!Resources.TryGetValue(pId, out string? resourcePath)) {
            return null;
        }

        return GD.Load<T>(resourcePath);
    }

    private static void LoadResources() {
        // Clear in case this class is created more than once.
        Resources.Clear();

        Logger.Core.Info("Loading Resource Database.");

        {
#if TOOLS
            using var clock = new Clock(_instance, "Resource Loading", true);
#endif
            LoadResourcesRecursive(ItemLocation, ItemPrefix, MaxRecursionDepth);
            LoadResourcesRecursive(SpellLocation, SpellPrefix, MaxRecursionDepth);
            LoadResourcesRecursive(EffectLocation, EffectPrefix, MaxRecursionDepth);
        }

        Logger.Core.Info("Done: Loading Resource Database.");

#if TOOLS
        if (Engine.IsEditorHint()) {
            Logger.Core.Info("Validating Resource Database.");

            // IMPORTANT: Validation must happen during editor hint, otherwise ResourceSaver will not save resources correctly.
            using var clock = new Clock(_instance, "Resource Validation", true);
            ValidateResources();

            Logger.Core.Info("Done: Validating Resource Database.");
        }
#endif
    }

    private static void LoadResourcesRecursive(string pPath, string pPrefix, int pDepth) {
        if (pDepth <= 0) {
            Logger.Core.Error($"Can't load a Resources from '{pPath}'. Max recursion depth reached!");
            return;
        }

        string[] contents = ResourceLoader.ListDirectory(pPath);
        if (contents.Length == 0) {
            Logger.Core.Error($"Error listing contents of '{pPath}'. Directory is either empty or does not exist!",
                true);
            return;
        }

        foreach (string item in contents) {
            string fullPath = pPath.PathJoin(item);

            if (item.EndsWith('/')) {
                LoadResourcesRecursive(fullPath, pPrefix, pDepth - 1);
                continue;
            }

            string id = FilePathToId(pPrefix, fullPath);

            if (Resources.TryGetValue(id, out string? existingPath)) {
                Logger.Core.Error(
                    $"Can't register '{fullPath}'. A resource with the same file name already exists at '{existingPath}'.");
                continue;
            }

            Resources[id] = fullPath;
        }
    }

    private static string FilePathToId(string pPrefix, string pFilePath) {
        return pPrefix + pFilePath.GetBaseName().GetFile().ToSnakeCase();
    }

#if TOOLS
    private static void ValidateResources() {
        foreach ((Id key, string fullPath) in Resources) {
            string id = key.ToString();

            if (id.StartsWith(ItemPrefix) || id.StartsWith(SpellPrefix)) {
                var gizmo = ResourceLoader.Load<Gizmo?>(fullPath);

                if (gizmo is null) {
                    Logger.Core.Error(
                        $"Could not load Resource from '{fullPath}'. It may not be a valid {nameof(Gizmo)}");
                    continue;
                }

                ValidateGizmo(gizmo, key, fullPath);

                Error error = ResourceSaver.Save(gizmo, fullPath);
                if (error != Error.Ok) {
                    Logger.Core.Error($"Could not save {nameof(Gizmo)} '{fullPath}'. Error code: {error}");
                }
            } else if (id.StartsWith(EffectPrefix)) {
                var effect = ResourceLoader.Load<Effect?>(fullPath);

                if (effect is null) {
                    Logger.Core.Error(
                        $"Could not load Resource from '{fullPath}'. It may not be a valid {nameof(Effect)}");
                    continue;
                }

                ValidateEffectId(effect);
                ValidateExcludingEffects(effect);

                Error error = ResourceSaver.Save(effect, fullPath);
                if (error != Error.Ok) {
                    Logger.Core.Error($"Could not save {nameof(Effect)} '{fullPath}'. Error code: {error}");
                }
            } else {
                Logger.Core.Error($"Unregistering resource of unknown type. Id = {id}, Path = {fullPath}");
            }
        }
    }

    private static void ValidateGizmo(Gizmo pGizmo, Id pId, string pFilePath) {
        ValidateGizmoId(pGizmo, pId, pFilePath);

        ValidateGizmoComponents(pGizmo);
    }

    private static void ValidateGizmoId(Gizmo pGizmo, Id pId, string pFilePath) {
        if (pGizmo.Id != pId) {
            pGizmo.SetProperty(nameof(Gizmo.Id), pId);
        }

        ValidateId(pId, pFilePath);
    }

    private static void ValidateId(Id pId, string pFilePath) {
        int colonCount = 0;
        bool detectedUpperCase = false;
        bool detectedNonPrintableASCII = false;

        foreach (char c in pId.ToString()) {
            if (c == ':') {
                ++colonCount;
            }

            if (char.IsUpper(c)) {
                detectedUpperCase = true;
            }

            if (char.IsControl(c) || char.IsWhiteSpace(c) || !char.IsAscii(c)) {
                detectedNonPrintableASCII = true;
            }
        }

        if (colonCount > 1) {
            Logger.Core.Warn($"Id '{pId}' of '{pFilePath}' resource should not contain more than 1 colon!");
        }

        if (detectedUpperCase) {
            Logger.Core.Warn($"Id '{pId}' of '{pFilePath}' resource should not contain upper case letters!");
        }

        if (detectedNonPrintableASCII) {
            Logger.Core.Warn($"Id '{pId}' of '{pFilePath}' must contain only printable ASCII characters!");
        }
    }

    private static void ValidateGizmoComponents(Gizmo pGizmo) {
        bool foundSpellComponent = false;
        bool foundChainSpellComponent = false;

        foreach ((string _, GizmoComponent? value) in pGizmo.Components) {
            switch (value) {
                case ChainSpellComponent chainSpellComponent:
                    foundChainSpellComponent = true;

                    ValidateEffects(pGizmo, chainSpellComponent.GetEffects());
                    ValidateSpellsOfChainSpell(pGizmo, chainSpellComponent);
                    ValidateLinkedSpells(pGizmo, chainSpellComponent);

                    break;
                case SpellComponent spellComponent:
                    foundSpellComponent = true;

                    ValidateEffects(pGizmo, spellComponent.GetEffects());
                    ValidateLinkedSpells(pGizmo, spellComponent);

                    break;
                case null:
                    Logger.Core.Warn($"{nameof(Gizmo)} '{pGizmo.ResourcePath}' has a null {nameof(GizmoComponent)}");
                    break;
            }
        }

        // Sanity check: since ChainSpellComponent and SpellComponent is weird, I need to warn myself to avoid making mistakes.
        // ChainSpellComponent will take priority over SpellComponent!
        if (foundSpellComponent && foundChainSpellComponent) {
            Logger.Core.Warn(
                $"{nameof(Gizmo)} '{pGizmo.ResourcePath}' should not contain both {nameof(SpellComponent)} and {nameof(ChainSpellComponent)}.");
        }
    }

    private static void ValidateSpellsOfChainSpell(Gizmo pGizmo, ChainSpellComponent pChainSpellComponent) {
        foreach (Gizmo spell in pChainSpellComponent.Spells) {
            if (spell.ResourcePath.Contains("::")) {
                Logger.Core.Warn(
                    $"{nameof(ChainSpellComponent)} in {nameof(Gizmo)} '{pGizmo.ResourcePath}' should not have built-in Spell(s). Instead save them to a file at '{SpellLocation}'.");
                break;
            }
        }
    }

    private static void ValidateLinkedSpells(Gizmo pGizmo, SpellComponent pSpellComponent) {
        foreach (Id linkedSpell in pSpellComponent.LinkedSpells) {
            if (!linkedSpell.ToString().StartsWith(SpellPrefix)) {
                Logger.Core.Error(
                    $"In {nameof(SpellComponent)} of '{pGizmo.ResourcePath}', linked spell '{linkedSpell}' does not start with '{SpellPrefix}' prefix.");
                continue;
            }

            if (!Resources.ContainsKey(linkedSpell)) {
                Logger.Core.Error(
                    $"In {nameof(SpellComponent)} of '{pGizmo.ResourcePath}', linked spell '{linkedSpell}' have not been registered.");
            }
        }
    }

    private static void ValidateEffects(Gizmo pGizmo, Effect?[] pEffects) {
        foreach (Effect? effect in pEffects) {
            if (effect is null) {
                Logger.Core.Warn($"{nameof(Gizmo)} '{pGizmo.ResourcePath}' has a null {nameof(Effect)}.");
                continue;
            }

            if (effect.ResourcePath.Contains("::")) {
                Logger.Core.Warn(
                    $"{nameof(Gizmo)} '{pGizmo.ResourcePath}' should not have built-in {nameof(Effect)}(s). Instead save them to a file at '{EffectLocation}'.");
                break;
            }
        }
    }

    private static void ValidateExcludingEffects(Effect pEffect) {
        foreach (Id id in pEffect.ExcludingEffects) {
            if (!id.ToString().StartsWith(EffectPrefix)) {
                Logger.Core.Error($"In {nameof(Effect)} '{pEffect.Id}', excluded id is not an effect id '{id}'");
            } else if (!Resources.ContainsKey(id)) {
                Logger.Core.Error($"{nameof(Effect)} '{pEffect.Id}' is excluding an effect with unknown id '{id}'");
            }
        }
    }

    private static void ValidateEffectId(Effect pEffect) {
        string id = FilePathToId(EffectPrefix, pEffect.ResourcePath);
        if (pEffect.Id != id) {
            pEffect.SetProperty(nameof(Effect.Id), new Id(id));
        }

        ValidateId(pEffect.Id, pEffect.ResourcePath);
    }

#endif
}