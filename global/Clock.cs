using Godot;
using System;
using System.Runtime.CompilerServices;
using RPG.global;

namespace RPG.Global;

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