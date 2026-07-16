using Godot;

namespace HydraTextClient.Scripts.Utility;

public partial class Highlighter : ColorRect
{
    [Export] public Color Idle = Colors.Transparent;
    [Export] public Color Hover = Colors.AliceBlue;
    [Export] public Control? HigherPower;
    private double Timer;
    private Tween Tween;

    public override void _Ready()
    {
        if (HigherPower is not null)
        {
            HigherPower.MouseEntered += Enter;
            HigherPower.MouseExited += Exit;
            return;
        }
        MouseEntered += Enter;
        MouseExited += Exit;
    }

    public void Enter()
    {
        Tween?.Kill();
        Tween = CreateTween();
        Tween.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        Tween.TweenProperty(this, "color", Hover, 1);
    }

    public void Exit()
    {
        Tween?.Kill();
        Tween = CreateTween();
        Tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        Tween.TweenProperty(this, "color", Idle, 1);
    }
}