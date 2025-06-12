using Godot;

namespace RPG.ui;

[GlobalClass]
public partial class CooldownDisplay : TextureProgressBar {
    private Tween? _tween;

    public void Start(double pCooldownInSeconds) {
        if (_tween is not null && _tween.IsValid()) {
            _tween.Kill();
        }
        
        Value = 100;
        _tween = CreateTween();
        _tween.TweenProperty(this, "value", Variant.From(0.0), pCooldownInSeconds);
    }

    public void Reset() {
        Value = 0;
        if (_tween is not null && _tween.IsValid()) {
            _tween.Kill();
        }
    }
}