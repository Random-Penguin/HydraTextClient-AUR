using Godot;
using static HydraTextClient.Scripts.Utility.ColorIdConstants;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public abstract partial class AnimatedMessageScene : MessageScene
{
    [Export] private float TextAnimationLength = 1;
    
    public IPrintableObj[] CompiledMessage;
    public IPrintableObj[] CompiledNameMessage;
    private bool StopUpdating;
    private double Timer;
    private Vector2 EndSize;
    private Tween Tween;
    private bool RunAnimation = false;
    
    public override void _Ready() => SetupMessage(true);

    public override void _Process(double delta)
    {
        if (!RunAnimation) return;
        Tween = CreateTween();
        Tween.SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
        Tween.TweenProperty(Message, "offset_transform_position", Vector2.Zero, 1.5).From(new Vector2(Message.Size.X * 2, 0));
        Tween.Parallel().TweenProperty(Message, "modulate:a", 1, 0);
        RunAnimation = false;
    }

    public void RunBounceAnimation() => RunAnimation = true;
    
    public override bool CanReload(string saveId)
    {
        if (saveId is PlayerConnect) return true;
        if (IdToConstant.ContainsKey(saveId)) return true;
        return false;
    }
}