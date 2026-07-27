using Archipelago.MultiClient.Net.Packets;
using Godot;
using HydraTextClient.Scripts.Clients.TextClient.ParserEffects;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class ClientMessage : AnimatedMessageScene
{
    [Export] private RichTextLabel PlayerName;
    [Export] private TextureRect GamePortrait;

    private string GameName;
    private string MessageText;
    private string Player;
    private int PlayerSlot;

    public override void SetInternalPacket(IMessagePacket packetBase)
    {
        if (packetBase.GetPacket() is not ChatPrintJsonPacket packet) return;
        if (!ConnectionController.HasLeaderClient) return;
        var leader = ConnectionController.LeaderClient!;

        GameName = leader.PlayerGames[packet.Slot];
        MessageText = packet.Message.Sanitize();
        Player = $"{{{{player;{PlayerSlot = packet.Slot}}}}}";

        SaveType<bool>.AddIndividualEvent(TextClient.ShowGamePortraits, CallReload);
        GamePortraitLoader.Singleton.OnReloadImages += CallReload;
        Reload();
        RunBounceAnimation();
    }

    public override void Reload()
    {
        if (GamePortraitLoader.Singleton.TryGet(GameName, out var gameImage)
            && SaveType<bool>.Load(TextClient.ShowGamePortraits, true))
        {
            GamePortrait.Visible = true;
            GamePortrait.Texture = gameImage;
        }
        else GamePortrait.Visible = false;

        UpdateFontSize(Message);
        UpdateFontSize(PlayerName);

        Message.Clear();
        PlayerName.Clear();

        Message.ApplyCompiledPrintableObjs(MessageText.CompileRichText(GetCompileEffects(), false));
        PlayerName.ApplyCompiledPrintableObjs(Player.CompileRichText(GetCompileEffects(), false));
    }

    public override string CopyText() => $"\"{MessageText}\"\n-# -{PlayerEffect.PlayerName(PlayerSlot, true, out _)}";
    public override void RemoveEvents()
    {
        SaveType<bool>.RemoveIndividualEvent(TextClient.ShowGamePortraits, CallReload);
        GamePortraitLoader.Singleton.OnReloadImages -= CallReload;
    }
}