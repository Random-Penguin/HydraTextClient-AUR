using System;
using System.Collections.Generic;
using Godot;
using HydraTextClient.Scripts.Clients.TextClient.ParserEffects;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public abstract partial class MessageScene : PanelContainer
{
    [Export] public Label TimeStamp;
    [Export] public RichTextLabel Message;

    public Dictionary<string, string> CachedReplacement;
    internal Dictionary<string, Action<RichTextLabel, string[]>>? CompileEffects;
    private string MultiWorld;

    public abstract void SetInternalPacket(IMessagePacket packetBase);
    public abstract void Reload();
    public abstract string CopyText();
    public abstract void RemoveEvents();

    public override void _Ready() => SetupMessage(false);

    public void SetPacket(IMessagePacket packetBase)
    {
        EmoteEffect.OnUpdate += CallReload;
        EntranceEffect.OnUpdate += CallReload;
        FoundEffect.OnUpdate += CallReload;
        ItemEffect.OnUpdate += CallReload;
        LocationEffect.OnUpdate += CallReload;
        NotFoundEffect.OnUpdate += CallReload;
        PlayerEffect.OnUpdate += CallReload;
        SaveType<bool>.AddIndividualEvent(TextClient.ShowTimestamps, TimeStamp.SetVisible);
        SaveType<double>.AddIndividualEvent(TextClient.FontSizeId, CallReload);
        SetInternalPacket(packetBase);
    }

    public void CallReload(bool _) => CallReload();
    public void CallReload(string _) => CallReload();
    public void CallReload(double _) => CallReload();
    public void CallReload() => CallDeferred("Reload");

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
    public void UpdateFontSize(Label label) => label.SetFontSizeOverride(SaveType<double>.Load(TextClient.FontSizeId, 20d));

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton button) return;
        if (!button.Pressed) return;
        if (button.ButtonIndex is not MouseButton.Left) return;
        DisplayServer.ClipboardSet(CopyText().Replace("\\n", "\n"));
    }

    protected override void Dispose(bool disposing)
    {
        EmoteEffect.OnUpdate -= CallReload;
        EntranceEffect.OnUpdate -= CallReload;
        FoundEffect.OnUpdate -= CallReload;
        ItemEffect.OnUpdate -= CallReload;
        LocationEffect.OnUpdate -= CallReload;
        NotFoundEffect.OnUpdate -= CallReload;
        PlayerEffect.OnUpdate -= CallReload;
        SaveType<bool>.RemoveIndividualEvent(TextClient.ShowTimestamps, TimeStamp.SetVisible);
        SaveType<double>.RemoveIndividualEvent(TextClient.FontSizeId, CallReload);
        RemoveEvents();
    }
}