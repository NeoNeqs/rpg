using Godot;
using System;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class Test : Resource {
    [Export]
    public Curve Cur {
        get => _cur ?? GD.Load<Curve>("res://new_curve.tres");
        set => _cur = value;
    }

    private Curve? _cur;
}