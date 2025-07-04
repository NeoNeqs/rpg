#if TOOLS
using System;
using System.Runtime.CompilerServices;
using Godot;

namespace RPG.global.tools;

/// <summary>
/// Measures the time a scope takes to execute.
/// </summary>
/// <example> Measure a method:
///     <code>
///         void Foo() {
///             using var clock = new Clock(this, "");
///         }
///     </code>
/// </example>
/// <example> Measure a scope:
///     <code>
///         void Foo() {
///             if (...some condition...) {
///                 using var clock = new Clock(this, "some condition");
///             }
///         }
///     </code>
/// </example>
public sealed class Clock : IDisposable {
    private readonly object _object;
    private readonly string _tag;
    private readonly string _method;
    private readonly int _line;
    private readonly bool _autoStart;

    private ulong _startTime;
    private ulong _accumulatedTime = 0;

    /// <param name="pObj">The object that created the Clock instance. Usually you should pass <c>this</c>.</param>
    /// <param name="pTag">The name of the scope that is being measured.</param>
    /// <param name="pAutoStart">Whether the Clock should immediately begin measuring or not.</param>
    /// <param name="pMethod">The name of the method from <paramref name="pObj" />, that created the Clock instance. Will be filled automatically by <see cref="System.Runtime.CompilerServices.CallerMemberNameAttribute"/>.</param>
    /// <param name="pLine">The line number from <paramref name="pObj" />, that created the Clock instance. Will be filled automatically by <see cref="System.Runtime.CompilerServices.CallerLineNumberAttribute"/>.</param>
    public Clock(
        object pObj,
        string pTag,
        bool pAutoStart,
        [CallerMemberName]
        string pMethod = "",
        [CallerLineNumber]
        int pLine = -1) {
        {
            _object = pObj;
            _tag = pTag;
            _method = pMethod;
            _line = pLine;
            _autoStart = pAutoStart;

            if (pAutoStart) {
                Start();
            }
        }
    }

    public void Start() {
        _startTime = Time.GetTicksUsec();
    }

    public void Stop() {
        ulong endTime = Time.GetTicksUsec();
        _accumulatedTime += endTime - _startTime;
    }

    public void ShowResult() {
        string humanizedTime = Utils.HumanizeMicroseconds(_accumulatedTime);
        Logger.Core.Info($"({_tag}) {_object.GetType().Name}::{_method}:{_line} -> {humanizedTime}");
    }

    public void Dispose() {
        if (_autoStart) {
            Stop();
        }

        ShowResult();
    }
}
#endif