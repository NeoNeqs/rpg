using Godot;
using System;
using System.Runtime.CompilerServices;

namespace RPG.global;

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
/// <param name="pObject">The object that created the Clock instance. Usually you should pass <c>this</c>.</param>
/// <param name="pTag">The name of the scope that is being measured.</param>
/// <param name="pMethod">The name of the method from <paramref name="pObject" />, that created the Clock instance. Will be filled automatically by <see cref="System.Runtime.CompilerServices.CallerMemberNameAttribute"/>.</param>
/// <param name="pLine">The line number from <paramref name="pObject" />, that created the Clock instance. Will be filled automatically by <see cref="System.Runtime.CompilerServices.CallerLineNumberAttribute"/>.</param>
public sealed class Clock(
    object pObject,
    string pTag,
    [CallerMemberName] string pMethod = "",
    [CallerLineNumber] int pLine = -1
) : IDisposable {
    private readonly ulong _startTime = Time.GetTicksUsec();

    private void Stop() {
        ulong endTime = Time.GetTicksUsec();
        string humanizedTime = Utils.HumanizeMicroseconds(endTime - _startTime);

        Logger.Core.Info($"({pTag}) {pObject.GetType().Name}::{pMethod}:{pLine} -> {humanizedTime}");
    }

    public void Dispose() {
        Stop();
    }
}