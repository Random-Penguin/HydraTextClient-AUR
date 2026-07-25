using Godot;

namespace HydraTextClient.Scripts.Utility.UIHelpers;

public partial class ButtonAnimation : Button
{
    private Tween Tween;
    private Vector2 SmolScale = new(.9f, .7f);
    private Vector2 NormalScale = new(1, 1);

    public override void _Ready()
    {
        PivotOffset = Size * new Vector2(.5f, 1);
        Resized += () => PivotOffset = Size * new Vector2(.5f, 1);
        ButtonDown += () =>
        {
            Scale = SmolScale;
            Tween?.Kill();
        };

        ButtonUp += () =>
        {
            Tween = CreateTween();
            Tween.SetTrans(Tween.TransitionType.Spring).SetEase(Tween.EaseType.Out);
            Tween.TweenProperty(this, "scale", NormalScale, .25f);
        };
    }
}