using System;
using System.Collections.Generic;
using Godot;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public abstract partial class MessageScene : PanelContainer
{
    public const string PlayerConnect = ";event;player connect";

    [Export] public Label TimeStamp;
    [Export] public RichTextLabel Message;

    public Dictionary<string, string> CachedReplacement;
    private Dictionary<string, Action<RichTextLabel, string[]>>? CompileEffects;
    private string MultiWorld;

    public abstract void SetPacket(IMessagePacket packetBase);
    public abstract void Reload();
    public abstract bool CanReload(string saveId);

    public override void _Ready() => SetupMessage(false);

    public void ReloadUi(string saveId)
    {
        if (ConnectionController.CurrentMultiworld != MultiWorld) return;
        if (!ConnectionController.HasLeaderClient) return;
        
        if (saveId.StartsWith("Clients/TextClient/TextEffects/") || saveId is PlayerConnect or TextClient.FontSizeId)
        {
            CallReload();
            return;
        }

        if (!CanReload(saveId)) return;
        CallReload();
    }

    private void CallReload() => CallDeferred("Reload");

    public void SetupMessage(bool transform)
    {
        MultiWorld = ConnectionController.CurrentMultiworld;
        Message.BbcodeEnabled = true;
        Message.FitContent = true;
        if (transform) Message.OffsetTransformEnabled = true;
    }

    public virtual Dictionary<string, Action<RichTextLabel, string[]>> GetCompileEffects()
    {
        if (CompileEffects is not null) return CompileEffects;
        return CompileEffects = MessageParser.CreateEffects(() => CallDeferred("Reload"));
    }

    public void UpdateFontSize(RichTextLabel label) => label.SetFontSizeOverride(SaveType<double>.Load(TextClient.FontSizeId, 20d));
}