using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using RPG.global;
using Godot;
using Godot.Collections;

namespace RPG.Global;

public class Logger(string pTag, Logger.Level pCurrentLevel) {
    public enum Level {
        Debug,
        Info,
        Warn,
        Error,
        Critical,
    }

    public static readonly Logger Core = new Logger("Core", Level.Debug);
    public static readonly Logger Inventory = new Logger("Inventory", Level.Debug);
    public static readonly Logger Combat = new Logger("Combat", Level.Debug);


    static Logger() {
        _PrintInfo();
    }

    [Conditional("DEBUG")]
    public void Debug(string pMessage, bool pVerbose = false) {
        _Log(Level.Debug, pMessage, pVerbose);
    }

    public void Info(string pMessage, bool pVerbose = false) {
        _Log(Level.Info, pMessage, pVerbose);
    }

    public void Warn(string pMessage, bool pVerbose = false) {
        _Log(Level.Warn, pMessage, pVerbose);
    }

    public void Error(string pMessage, bool pVerbose = false) {
        _Log(Level.Error, pMessage, pVerbose);
    }

    public void Critical(string pMessage, bool pVerbose = false) {
        _Log(Level.Critical, pMessage, pVerbose);
        
        System.Diagnostics.Debug.Assert(false, pMessage);
    }

    private void _LogStackTrace() {
        var stackTrace = new StackTrace();
        // Slice off the first 2 entries (call to `_LogStacktrace()` and `_Log()`)
        foreach (StackFrame frame in stackTrace.GetFrames().Skip(2)) {
            var trace = $"\tat: {frame.GetFileName()} -> {frame.GetMethod()?.Name}:{frame.GetFileLineNumber()}";
            GD.PrintRich(_FormatColor(trace, pCurrentLevel));
        }
    }

    private static string _GetThreadString() {
        return OS.GetMainThreadId() == OS.GetThreadCallerId() ? "Main" : $"Thread/{OS.GetThreadCallerId()}";
    }

    private static string _GetDatetimeNow() {
        double unixTime = Time.GetUnixTimeFromSystem();
        var unixTimeSec = (int)unixTime;
        double milliseconds = unixTime - unixTimeSec;

        string formatted = Time.GetDatetimeStringFromUnixTime(unixTimeSec, true);

        return string.Concat(formatted.AsSpan(), milliseconds.ToString(CultureInfo.CurrentCulture).AsSpan(1, 6));
    }

    private static string _FormatColor(string pMessage, Level pLevel) {
        string color = pLevel switch {
            Level.Debug => "cyan",
            Level.Info => "green",
            Level.Warn => "yellow",
            Level.Error => "orangered",
            Level.Critical => "red",
            _ => throw new ArgumentOutOfRangeException(nameof(pLevel), pLevel, "Unhandled Logger.Level found.")
        };

        return $"[color={color}] {pMessage}[/color]";
    }

    private void _Log(Level pLevel, string pMessage, bool pVerbose) {
        if (pLevel < pCurrentLevel) {
            return;
        }

        string datetime = _GetDatetimeNow();
        var levelStr = pLevel.ToString();
        string threadStr = _GetThreadString();

        var formattedMessage = $"[{datetime}] [{levelStr}] [{pTag}] [{threadStr}]: {pMessage}";

        if (OS.HasFeature("editor")) {
            GD.PrintRich(_FormatColor(formattedMessage, pLevel));
        } else {
            GD.Print(formattedMessage);
        }

        if (pVerbose) {
            _LogStackTrace();
        }
    }

    static void _PrintInfo() {
        if (!OS.HasFeature("template")) {
            return;
        }

        string deviceName = RenderingServer.GetRenderingDevice().GetDeviceName();
        string renderingMethod = RenderingServer.GetCurrentRenderingMethod();
        string driverName = RenderingServer.GetCurrentRenderingDriverName();
        string apiVersion = RenderingServer.GetVideoAdapterApiVersion();

        Dictionary memInfo = OS.GetMemoryInfo();
        var available = (int)memInfo["available"];
        var physical = (int)memInfo["physical"];
        var stack = (int)memInfo["stack"];
        int swap = available - physical;

        Core.Info("--------------------System Information--------------------");
        Core.Info($"OS: {OS.GetDistributionName()} {OS.GetVersion()}, Locale: {OS.GetLocale()}");
        Core.Info($"CPU: {OS.GetProcessorName()}, ({OS.GetProcessorCount()}-core)");
        Core.Info($"GPU: {deviceName}, {renderingMethod} API: {driverName} ({apiVersion})");
        Core.Info($"Memory: {Utils.HumanizeBytes(physical)} + {Utils.HumanizeBytes(swap)} Swap");
        Core.Info($"Stack size: {Utils.HumanizeBytes(stack)}");

        Core.Info("--------------------Process Information--------------------");
        Core.Info($"Executable path: {OS.GetExecutablePath()}");
        Core.Info($"Engine arguments: {OS.GetCmdlineArgs()}");
        Core.Info($"User arguments: {OS.GetCmdlineUserArgs()}");

        Core.Info("--------------------Storage Information--------------------");
        Core.Info($"Is 'user://' persistent: {OS.IsUserfsPersistent()}");
        Core.Info($"User data dir: {OS.GetUserDataDir()}");
        Core.Info($"Config dir: {OS.GetConfigDir()}");
        Core.Info($"Cache dir: {OS.GetCacheDir()}");
        Core.Info($"Data dir: {OS.GetDataDir()}");

        Core.Info("--------------------Misc Information--------------------");
        Core.Info($"Is sandboxed: {OS.IsSandboxed()}");
    }
}