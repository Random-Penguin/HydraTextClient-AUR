using System;
using Godot;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Connection.Slots;

public partial class SlotPortrait : TextureRect
{
    [ExportGroup("Internal"), Export] private Texture2D UnknownPortrait;
    [Export] private TextureRect Portrait;
    [Export] private Label SlotNameLabel;

    [ExportGroup("Internal - CheckCount"), Export]
    private PanelContainer CheckCountPanel;

    [Export] private Label CheckCountLabel;
    [Export] private ProgressBar CheckProgressBar;

    [ExportGroup("Internal - Tinter"), Export]
    private ColorRect Tinter;

    [Export] private Color IdleTint;
    [Export] private Color ConnectingTint;
    [Export] private Color ConnectedTint;
    [Export] private Color ErrorTint;

    [Signal] public delegate void OnPortraitLeftClickedEventHandler(string slotName);

    [Signal] public delegate void OnPortraitRightClickedEventHandler(string slotName);

    public string SlotName;
    public string GameName;
    private Vector2 PortraitSize = new(150, 225);
    private Action<string, int, int> CheckAction;
    private Action ClearCheckCountOnDisconnect;
    private Tween ColorTween;
    private Tween FontSizeTween;
    private Tween ScaleTween;

    public override void _Ready()
    {
        CheckAction = (slot, amount, max) =>
        {
            var mw = ConnectionController.GetCurrentMultiworld;
            if (mw is null) return;
            var player = mw.GetSlotName(slot);
            var thisPlayer = mw.GetSlotName(SlotName);
            if (thisPlayer != player)
            {
                if (mw.CheckCounts.TryGetValue(thisPlayer, out max)
                    && mw.CheckCountsChecked.TryGetValue(thisPlayer, out amount))
                {
                    CallDeferred("UpdateCheckCount", amount, max);
                }

                return;
            }
            CallDeferred("UpdateCheckCount", amount, max);
        };

        ClearCheckCountOnDisconnect = () => CheckCountPanel.Visible = false;

        CheckCountPanel.Visible = false;
        SetFontSize((int)SaveType<double>.Load("Connection/SlotsMenu/PortraitFontSize", 14));
        SetScale((float)SaveType<double>.Load("Connection/SlotsMenu/PortraitScale", 1f));
        ConnectionController.OnCheckCountUpdated += CheckAction;
        ConnectionController.OnFullDisconnection += ClearCheckCountOnDisconnect;

        Reload();
    }

    public void Reload()
    {
        if (!SaveType<SlotGameData>.TryGet(SlotName, out var data))
        {
            QueueFree();
            return;
        }

        SlotNameLabel.Text = SlotName;
        Portrait.Texture = GamePortraitLoader.GetOrDef(GameName = data.Game, UnknownPortrait);
    }

    public void SetScale(float scale)
    {
        var newSize = PortraitSize * scale;
        ScaleTween?.Kill();
        ScaleTween = CreateTween();
        ScaleTween.SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
        ScaleTween.TweenProperty(this, "custom_minimum_size", newSize, .7f);
        ScaleTween.Parallel().TweenProperty(this, "size", newSize, .7f);
        SetSize(newSize);
    }

    public void SetFontSize(int size)
    {
        FontSizeTween?.Kill();
        FontSizeTween = CreateTween();
        FontSizeTween.SetTrans(Tween.TransitionType.Elastic).SetEase(Tween.EaseType.Out);
        FontSizeTween.TweenProperty(SlotNameLabel, "theme_override_font_sizes/font_size", size, .7f);
        FontSizeTween.Parallel().TweenProperty(CheckCountLabel, "theme_override_font_sizes/font_size", size, .7f);
        FontSizeTween.Parallel().TweenProperty(CheckProgressBar, "theme_override_font_sizes/font_size", size, .7f);
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton button) return;
        if (!button.Pressed) return;
        switch (button.ButtonIndex)
        {
            case MouseButton.Left: EmitSignalOnPortraitLeftClicked(SlotName); break;
            case MouseButton.Right: EmitSignalOnPortraitRightClicked(SlotName); break;
        }
    }

    public void SetStatus(ConnectionStatus status) => CallDeferred("TweenStatus", (int)status);

    private void TweenStatus(int intStatus)
    {
        var status = (ConnectionStatus)intStatus;
        ColorTween?.Kill();
        ColorTween = CreateTween();
        ColorTween.SetTrans(Tween.TransitionType.Circ).SetEase(Tween.EaseType.Out);
        switch (status)
        {
            case ConnectionStatus.Connecting:
                ColorTween.TweenProperty(Tinter, "color", ConnectingTint, 1);
                ColorTween.SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.In);
                ColorTween.TweenProperty(Tinter, "color", IdleTint, 1);
                ColorTween.SetLoops();
                break;
            case ConnectionStatus.NotConnected or ConnectionStatus.Connected or ConnectionStatus.Error:
                ColorTween.TweenProperty(
                    Tinter, "color",
                    status switch
                    {
                        ConnectionStatus.NotConnected => IdleTint, ConnectionStatus.Connected => ConnectedTint,
                        ConnectionStatus.Error => ErrorTint,
                    }, 1
                ); break;
        }
    }

    public void UpdateCheckCount(int count, int max)
    {
        CheckCountPanel.Visible = false;
        if (!ConnectionController.HasLeaderClient) return;
        var mw = ConnectionController.GetCurrentMultiworld;
        if (mw is null) return;

        var slot = mw.GetSlotName(SlotName);
        var leader = ConnectionController.LeaderClient;
        if (!leader!.PlayerNames.Contains(slot)) return;

        CheckCountPanel.Visible = true;
        CheckCountLabel.Text = $"{count:###,##0}/{max:###,##0}";
        CheckProgressBar.Value = (float)count / max;
    }

    protected override void Dispose(bool disposing)
    {
        ConnectionController.OnCheckCountUpdated -= CheckAction;
        ConnectionController.OnFullDisconnection -= ClearCheckCountOnDisconnect;
    }
}

public enum ConnectionStatus
{
    NotConnected, Connecting, Connected,
    Error
}