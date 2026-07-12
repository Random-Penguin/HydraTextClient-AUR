using System;
using System.Collections.Generic;
using Godot;
using HydraTextClient.Scripts.Clients.TextClient;
using HydraTextClient.Scripts.Utility;

namespace HydraTextClient.Scripts.PlayerList;

public partial class PlayerItem : PanelContainer
{
    [Export] private Gradient CheckGradient;
    [Export] private Color GoalColor;
    [Export] private RichTextLabel Player;
    [Export] private Label CheckCounter;
    [Export] private Label CheckIndicator;
    [Export] private ProgressBar CheckProgress;
    [Export] private TextureRect ConnectedIndicator;
    [Export] private TextureRect DisconnectedIndicator;
    [Export] private TextureRect GoalIndicator;

    private Dictionary<string, Action<RichTextLabel, string[]>> Effects;

    private string PlayerText;
    private bool Goaled;
    private Tween ProgressTween;
    private Tween CheckGainTween;
    private int LastCount = -1;

    public override void _Ready()
    {
        CheckProgress.Modulate = CheckGradient.Sample(0);
        Effects = MessageParser.CreateEffects(() => CallDeferred("UpdatePlayerText"));
    }

    public void SetPlayer(int player)
    {
        PlayerText = $" {{{{player;{player}}}}}";
        UpdatePlayerText();
        SetCheckCount();
    }

    public void UpdatePlayerText()
    {
        Player.Clear();
        Player.ApplyCompiledPrintableObjs(PlayerText.CompileRichText(Effects, false));
    }

    public void SetCheckCount(int count = 0, int max = 0)
    {
        if (max <= 0 && !Goaled)
        {
            CheckProgress.Visible = false;
            CheckCounter.Text = "";
            return;
        }

        CheckProgress.Visible = true;

        ProgressTween?.Kill();
        if (Goaled)
        {
            if (LastCount == -2) return;
            ProgressTween = CreateTween();
            ProgressTween.SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
            SetConnected(null);
            GoalIndicator.Visible = true;
            CheckCounter.Text = "Goaled ";

            ProgressTween.TweenProperty(CheckProgress, "value", 100, 1);
            ProgressTween.Parallel().TweenProperty(CheckProgress, "modulate", GoalColor, 1);
            LastCount = -2;
            return;
        }

        if (LastCount == count) return;
        LastCount = count;
        ProgressTween = CreateTween();
        ProgressTween.SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);
        var normalized = (double)count / max;
        CheckCounter.Text = $"{count:###,##0}/{max:###,##0} ({normalized * 100d:#00.00}%)";

        ProgressTween.TweenProperty(CheckProgress, "value", normalized * 100d, 1);
        ProgressTween.Parallel()
                     .TweenProperty(CheckProgress, "modulate", CheckGradient.Sample((float)normalized), 1);

        CheckGainTween?.Kill();
        CheckGainTween = CreateTween();
        CheckGainTween.SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.InOut);
        CheckGainTween.TweenProperty(CheckIndicator, "modulate:a", 0, 3).From(1);
    }

    public void HasGoaled()
    {
        if (Goaled) return;
        Goaled = true;
        SetCheckCount();
    }

    public void SetConnected(bool? isConnected)
    {
        if (isConnected is null) DisconnectedIndicator.Visible = ConnectedIndicator.Visible = false;
        else DisconnectedIndicator.Visible = !(ConnectedIndicator.Visible = isConnected!.Value);
    }
}