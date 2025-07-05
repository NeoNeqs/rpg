using System;
using System.Diagnostics;
using System.Globalization;
using Godot;


namespace RPG.global;

/// <summary>
/// Abstraction on top of Godot's print functions with additional logging features:
/// <para> 1. Logging current date and time </para>
/// <para> 2. Logging severity levels </para>
/// <para> 3. Named loggers </para>
/// <para> 4. Logging from different threats </para>
/// <para> 5. Colorful output </para>
/// <para> 6. Predictable output, format should easily be parsable with a regex </para>
/// <para> 7. Easy to sort / filter </para>
/// </summary>
/// <param name="pTag">The general tag (a name) that defines what this logger is related to.</param>
/// <param name="pCurrentLevel">Controls the minimum allowed level for logging. Any logging below <paramref name="pCurrentLevel"/> will be ignored.</param>
public sealed class Logger(string pTag, Logger.Level pCurrentLevel) {
    public enum Level {
        /// Used only for debugging purposes, should not be present in release builds 
        Debug,

        /// Useful information that require no action.
        Info,

        /// A warning usually doesn't require action, indicates a misconfiguration or a soft error that can be ignored.
        Warn,

        /// An error requires an action.
        Error,

        /// A more serious error, usually triggered in unrecoverable state and so it may follow a crash.
        Critical,
    }

    // TODO: Decide on levels for release builds:
#if DEBUG
    public static readonly Logger Core = new("Core", Level.Debug);
    public static readonly Logger Inventory = new("Inventory", Level.Debug);
    public static readonly Logger Combat = new("Combat", Level.Debug);
    public static readonly Logger UI = new("UI", Level.Debug);
#else
    public static readonly Logger Core = new("Core", Level.Info);
    public static readonly Logger Inventory = new("Inventory", Level.Info);
    public static readonly Logger Combat = new("Combat", Level.Info);
    public static readonly Logger UI = new("UI", Level.Info);
#endif

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
    }

    private void _LogStackTrace() {
        StackFrame[] frames = new StackTrace(true).GetFrames();

        // Slice off the first 3 entries (call to `_LogStacktrace()` and `_Log()` and the public caller e.g. `Debug()`)
        // Also get rid off last few entries added by Godot's glue code.
        for (int i = 3; i < frames.Length - 5; ++i) {
            StackFrame frame = frames[i];
            string trace = $"\tat: {frame.GetFileName()} -> {frame.GetMethod()?.Name}:{frame.GetFileLineNumber()}";
            GD.PrintRich(_FormatColor(trace, pCurrentLevel));
        }
    }

    private static string _GetThreadString() {
        return OS.GetMainThreadId() == OS.GetThreadCallerId() ? "Main" : $"Thread/{OS.GetThreadCallerId()}";
    }

    private static string _GetDatetimeNow() {
        double unixTime = Time.GetUnixTimeFromSystem();
        int unixTimeSec = (int)unixTime;
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

        return $"[color={color}]{pMessage}[/color]";
    }

    private void _Log(Level pLevel, string pMessage, bool pVerbose) {
        if (pLevel < pCurrentLevel) {
            return;
        }

        string datetime = _GetDatetimeNow();
        string levelStr = pLevel.ToString();
        string threadStr = _GetThreadString();

        string formattedMessage = $"[{datetime}] [{levelStr}] [{pTag}] [{threadStr}]: {pMessage}";

#if TOOLS
        GD.PrintRich(_FormatColor(formattedMessage, pLevel));
#else
        GD.Print(formattedMessage);
#endif

        if (pVerbose) {
            _LogStackTrace();
        }
    }

#if !TOOLS
    static Logger() {
        _PrintInfo();
    }

    private static void _PrintInfo() {
        string deviceName = RenderingServer.GetRenderingDevice().GetDeviceName();
        string renderingMethod = RenderingServer.GetCurrentRenderingMethod();
        string driverName = RenderingServer.GetCurrentRenderingDriverName();
        string apiVersion = RenderingServer.GetVideoAdapterApiVersion();

        Godot.Collections.Dictionary memInfo = OS.GetMemoryInfo();
        ulong available = memInfo["available"].AsUInt64();
        ulong physical = memInfo["physical"].AsUInt64();
        ulong stack = memInfo["stack"].AsUInt64();
        ulong swap = available - physical;

        Core.Info("--------------------System Information--------------------");
        Core.Info($"OS: {OS.GetDistributionName()} {OS.GetVersion()}, Locale: {OS.GetLocale()}");
        Core.Info($"CPU: {OS.GetProcessorName()}, ({OS.GetProcessorCount()}-core)");
        Core.Info($"GPU: {deviceName}, {renderingMethod} API: {driverName} ({apiVersion})");
        Core.Info($"Memory: {Utils.HumanizeBytes(physical)} + {Utils.HumanizeBytes(swap)} Swap");
        Core.Info($"Stack size: {Utils.HumanizeBytes(stack)}");

        Core.Info("--------------------Process Information-------------------");
        Core.Info($"Executable path: {OS.GetExecutablePath()}");
        Core.Info($"Engine arguments: [{string.Join(',', OS.GetCmdlineArgs())}]");
        Core.Info($"User arguments: [{string.Join(',', OS.GetCmdlineUserArgs())}]");

        Core.Info("--------------------Storage Information-------------------");
        Core.Info($"Is 'user://' persistent: {OS.IsUserfsPersistent()}");
        Core.Info($"User data dir: {OS.GetUserDataDir()}");
        Core.Info($"Config dir: {OS.GetConfigDir()}");
        Core.Info($"Cache dir: {OS.GetCacheDir()}");
        Core.Info($"Data dir: {OS.GetDataDir()}");

        Core.Info("---------------------Misc Information---------------------");
        Core.Info($"Is sandboxed: {OS.IsSandboxed()}");
        
        Core.Info("--------------------End of Information--------------------");
    }
#endif
}