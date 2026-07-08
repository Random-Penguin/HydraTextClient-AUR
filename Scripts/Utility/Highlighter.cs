using Godot;

namespace HydraTextClient.Scripts.Utility;

public partial class Highlighter : ColorRect
{
    [Export] public Color Idle = Colors.Transparent;
    [Export] public Color Hover = Colors.AliceBlue;
    private double Timer;
    private Tween Tween;

    public override void _Ready()
    {
        MouseEntered += () =>
        {
            Tween?.Kill();
            Tween = CreateTween();
            Tween.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
            Tween.TweenProperty(this, "color", Hover, 1);
        };

        MouseExited += () =>
        {
            Tween?.Kill();
            Tween = CreateTween();
            Tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
            Tween.TweenProperty(this, "color", Idle, 1);
        };
    }
}