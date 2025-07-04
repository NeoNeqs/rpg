using System.Collections.Generic;
using Godot;
using RPG.global.tools;
using RPG.scripts;
using RPG.scripts.effects;
using RPG.scripts.inventory;
using RPG.scripts.inventory.components;
using Id = Godot.StringName;

namespace RPG.global.singletons;

// TODO: getEffect, getItem, etc.

[Tool]
public sealed partial class ResourceDB : Node {
    private static readonly Dictionary<Id, string> Resources = [];

    public static ResourceDB Instance = null!;

    private const int MaxRecursionDepth = 2;
    private const string ItemLocation = "res://resources/items/";
    private const string SpellLocation = "res://resources/spells/";
    private const string EffectLocation = "res://resources/effects/";

    public ResourceDB() {
        // ReSharper disable once NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
        Instance ??= this;
    }

    public override void _EnterTree() {
        LoadResources();
    }

    private static void LoadResources() {
        Logger.Core.Info("Loading Resource Database.");

        Resources.Clear();

        LoadResourcesRecursive<Gizmo>(
            ItemLocation,
            "item:",
            MaxRecursionDepth
        );

        LoadResourcesRecursive<Gizmo>(
            SpellLocation,
            "spell:",
            MaxRecursionDepth
        );

        LoadResourcesRecursive<Effect>(
            EffectLocation,
            "effect:",
            MaxRecursionDepth
        );

        Logger.Core.Info("Loaded Resource Database.");
    }

    private static void LoadResourcesRecursive<T>(
        string pPath,
        string pPrefix,
        int pDepth
    ) where T : Resource, INamedIdentifiable {
        if (pDepth <= 0) {
            Logger.Core.Error($"Can't load a {typeof(T).Name} from '{pPath}'. Max recursion depth reached!");
            return;
        }

        string[] contents = ResourceLoader.ListDirectory(pPath);
        if (contents.Length == 0) {
            Logger.Core.Error($"Error listing contents of '{pPath}'. Directory is either empty or does not exist!",
                true);
            return;
        }

#if TOOLS
        using var clock = new Clock(Instance, $"Resource Validation of {pPrefix}{typeof(T).Name}s", false);
#endif

        foreach (string item in contents) {
            string fullPath = pPath.PathJoin(item);

            if (item.EndsWith('/')) {
                LoadResourcesRecursive<T>(fullPath, pPrefix, pDepth - 1);
                continue;
            }

            Resource? resource = ResourceLoader.Load(fullPath, typeof(T).Name);

            if (resource == null) {
                Logger.Core.Error(
                    $"Could not load {typeof(T).Name} from '{fullPath}'. It may not be a valid {typeof(T).Name}.");
                continue;
            }

            if (resource is not T t) {
                Logger.Core.Error($"Resource '{fullPath}' is not {typeof(T).Name}.");
                continue;
            }

#if TOOLS
            if (Engine.IsEditorHint()) {
                clock.Start();
                ValidateResourceId(t, fullPath, pPrefix);

                if (t is Gizmo gizmo) {
                    ValidateGizmo(gizmo);
                }

                Error error = ResourceSaver.Save(t, fullPath);
                if (error != Error.Ok) {
                    Logger.Core.Error($"Could not save '{fullPath}'. Error code: {error}");
                    continue;
                }
                clock.Stop();
            }
#endif

            var id = resource.GetProperty<Id>(nameof(INamedIdentifiable.Id));
            if (id is null) {
                continue;
            }

            if (Resources.TryGetValue(id, out string? existingPath)) {
                Logger.Core.Error($"'{fullPath}' and '{existingPath}' have the same {typeof(T).Name} Id!");
                continue;
            }

            Resources[id] = fullPath;
        }
    }

#if TOOLS
    private static void ValidateGizmo(Gizmo pGizmo) {
        bool foundSpellComponent = false;
        bool foundChainSpellComponent = false;

        foreach ((string _, GizmoComponent? value) in pGizmo.Components) {
            switch (value) {
                case ChainSpellComponent chainSpellComponent:
                    foundChainSpellComponent = true;
                    ValidateEffects(pGizmo, chainSpellComponent.Effects);

                    break;
                case SpellComponent spellComponent:
                    foundSpellComponent = true;
                    ValidateEffects(pGizmo, spellComponent.Effects);
                    break;
                case null:
                    Logger.Core.Warn($"{nameof(Gizmo)} '{pGizmo.ResourcePath}' has a null {nameof(GizmoComponent)}");
                    break;
            }
        }

        if (foundSpellComponent && foundChainSpellComponent) {
            Logger.Core.Warn(
                $"{nameof(Gizmo)} '{pGizmo.ResourcePath}' should not contain both {nameof(SpellComponent)} and {nameof(ChainSpellComponent)}.");
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
                    $"{nameof(Gizmo)} '{pGizmo.ResourcePath}' should not have built-in {nameof(Effect)}(s). Instead save them to a file at '{SpellLocation}'.");
                break;
            }
        }
    }

    private static void ValidateResourceId<T>(T pNamedResource, string pFilePath, string pPrefix)
        where T : Resource, INamedIdentifiable {
        var id = pNamedResource.GetProperty<Id>(nameof(INamedIdentifiable.Id));

        if (id is null) {
            Logger.Core.Error(
                $"Resource '{pNamedResource.GetType().Name}' from '{pNamedResource.ResourceName}' has no valid Id property.");
            return;
        }

        string idStr = id.ToString();
        bool isIdEmpty = id.IsEmpty;

        if (!idStr.StartsWith(pPrefix)) {
            idStr = pPrefix + idStr;
        }

        if (isIdEmpty) {
            string? name = pNamedResource.GetProperty<string>(nameof(INamedIdentifiable.DisplayName));
            idStr = name?.Length > 0 ? idStr + name.ToSnakeCase() : idStr + pFilePath.GetBaseName().GetFile();
        }

        pNamedResource.SetProperty(nameof(INamedIdentifiable.Id), new Id(idStr));

        int colonCount = 0;
        bool detectedUpperCase = false;
        bool detectedNonPrintableASCII = false;

        foreach (char c in idStr) {
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
            Logger.Core.Warn($"Id of '{pFilePath}' resource contains more than 1 colon!");
        }

        if (detectedUpperCase) {
            Logger.Core.Warn($"Id of '{pFilePath}' resource contains upper case letters!");
        }

        if (detectedNonPrintableASCII) {
            Logger.Core.Warn($"Id of '{pFilePath}' must contain only printable ASCII characters!");
        }
    }
#endif
}