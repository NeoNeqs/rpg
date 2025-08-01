using System;
using System.Collections.Generic;
using System.Text;
using global::RPG.global;
using Godot;
using RPG.world;
using RPG.world.character;
using RPG.world.entity;

namespace RPG.ui.debug;

public partial class DebugConsole : PanelContainer {
    private LineEdit Prompt => GetNode<LineEdit>("MarginContainer/VBoxContainer/LineEdit");

    public new void GrabFocus() {
        Prompt.GrabFocus();
    }

    private void OnCommandSubmitted(string pNewText) {
        string[] command = ParseArguments(pNewText);
        if (command.Length == 0) {
            return;
        }

        Span<string> args = command.AsSpan(1);

        switch (command[0]) {
            case "tp":
                if (args.Length < 3) {
                    Logger.Core.Error("Command 'tp' requires 3 arguments!");
                    return;
                }

                HandleTeleport(args);
                break;
            case "spawn":
                if (args.Length < 1) {
                    Logger.Core.Error("Command 'spawn' requires 1 argument!");
                    return;
                }

                HandleSpawn(args);
                break;
            case "inspect":
                HandleInspect(args);
                break;
            case "modify":
                HandleModify(args);
                break;
        }

        Prompt.Clear();
    }

    private void HandleTeleport(Span<string> pArgs) {
        if (GetTree().CurrentScene is not Main main) {
            Logger.Core.Error($"Current scene is not {nameof(Main)}.");
            return;
        }

        World world = main.GetWorld();
        PlayerCharacter? player = world.GetPlayer();
        if (player is null) {
            Logger.Core.Error("Player does not exist in World.");
            return;
        }

        bool xOk = TryParseCoordinates(pArgs[0], out float x, out bool isXRelative);
        bool yOk = TryParseCoordinates(pArgs[1], out float y, out bool isYRelative);
        bool zOk = TryParseCoordinates(pArgs[2], out float z, out bool isZRelative);

        if (!xOk) {
            Logger.Core.Error($"X={pArgs[0]} is not a valid coordinate.");
            return;
        }

        if (!yOk) {
            Logger.Core.Error($"Y={pArgs[1]} is not a valid coordinate.");
            return;
        }

        if (!zOk) {
            Logger.Core.Error($"Z={pArgs[2]} is not a valid coordinate.");
            return;
        }

        Vector3 pos = player.GlobalPosition;

        pos.X = isXRelative ? pos.X + x : x;
        pos.Y = isYRelative ? pos.Y + y : y;
        pos.Z = isZRelative ? pos.Z + z : z;

        player.GlobalPosition = pos;
        return;

        // Local helper to parse value and determine relativity
        static bool TryParseCoordinates(string pInput, out float pValue, out bool pIsRelative) {
            pIsRelative = false;
            if (pInput.StartsWith('~')) {
                pIsRelative = true;
                pInput = pInput[1..];
            }

            if (string.IsNullOrEmpty(pInput)) {
                pValue = 0;
                return true;
            }

            return float.TryParse(pInput, out pValue);
        }
    }

    private void HandleSpawn(Span<string> pArgs) {
        if (GetTree().CurrentScene is not Main main) {
            Logger.Core.Error($"Current scene is not {nameof(Main)}.");
            return;
        }

        World world = main.GetWorld();

        PlayerCharacter? player = world.GetPlayer();
        if (player is null) {
            Logger.Core.Error("Player does not exist in World.");
            return;
        }

        // TODO: once there is EntityDB use pArgs[0] to spawn specific entities

        Vector3 position = world.GetMouseWorldPosition(100);
        var entity = GD.Load<PackedScene>("uid://blk21fp6k13se").Instantiate<Enemy>();
        entity.Position = position;
        world.AddChild(entity);
        entity.LookAt(player.GlobalPosition, Vector3.Up);
    }

    private void HandleModify(Span<string> pArgs) {
        bool isUlong = ulong.TryParse(pArgs[0], out ulong id);
        if (!isUlong) {
            Logger.Core.Error($"'{pArgs[0]}' is not a valid long number.");
            return;
        }

        GodotObject? obj = InstanceFromId(id);
        if (obj is null) {
            Logger.Core.Error("Object does not exist.");
            return;
        }

        GD.Print(pArgs[1]);
        Variant currentValue = obj.Get(pArgs[1]);
        switch (currentValue.VariantType) {
            case Variant.Type.Int:
                bool isBool = long.TryParse(pArgs[2], out long newValue);
                if (!isBool) {
                    Logger.Core.Error($"'{pArgs[2]}' is not a valid long!");
                }

                obj.Set(pArgs[1], Variant.From(newValue));
                GD.Print(Variant.From(newValue));
                break;
            case Variant.Type.String:
                obj.Set(pArgs[1], pArgs[2]);
                GD.Print("String");
                break;
        }
        // obj.Set(pArgs[1],);
    }

    private void HandleInspect(Span<string> pArgs) {
        // if (GetTree().CurrentScene is not Main main) {
        //     Logger.Core.Error($"Current scene is not {nameof(Main)}.");
        //     return;
        // }
        //
        // World world = main.GetWorld();
        //
        // Dictionary result = world.IntersectRay(1_000.0f, uint.MaxValue);
        //
        // if (result.Count == 0) {
        //     Logger.Core.Info("No object found at the mouse location.");
        //     return;
        // }
        //
        // GodotObject obj = result["collider"].AsGodotObject();
        // Logger.Core.Info($"{obj.Get("name").AsString()}= {obj.GetInstanceId()}");
        // Logger.Core.Info(obj.Get("faction").AsInt64().ToString());
        // foreach (Dictionary dictionary in obj.GetPropertyList()) {
        //    Logger.Core.Debug($"{dictionary["name"].AsString()}= {obj.Get(dictionary["name"].AsStringName())}");
        // }
    }

    private static string[] ParseArguments(string pInput) {
        var args = new List<string>();

        bool inQuotes = false;
        char quoteChar = '\0';

        var currentArg = new StringBuilder();

        for (int i = 0; i < pInput.Length; i++) {
            char c = pInput[i];

            // Start or end of quoted section
            if (c is '"' or '\'' && (i == 0 || pInput[i - 1] != '\\')) {
                if (!inQuotes) {
                    inQuotes = true;
                    quoteChar = c;
                } else if (c == quoteChar) {
                    inQuotes = false;
                } else {
                    currentArg.Append(c);
                }
            } else if (char.IsWhiteSpace(c) && !inQuotes) {
                if (currentArg.Length > 0) {
                    args.Add(currentArg.ToString());
                    currentArg.Clear();
                }
            } else {
                currentArg.Append(c);
            }
        }

        if (currentArg.Length > 0) {
            args.Add(currentArg.ToString());
        }

        return args.ToArray();
    }
}