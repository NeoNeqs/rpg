using System.Diagnostics;
using Godot;

namespace RPG.global.tools;

public sealed class Tools {

    [Conditional("TOOLS")]
    public static void Assert(bool pCondition, string pMessage = "") {
        if (!pCondition) {
            GD.PushError(pMessage);
        }
    }
    
}