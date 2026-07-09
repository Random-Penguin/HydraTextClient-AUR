using Archipelago.MultiClient.Net.Packets;
using Godot;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class ClientMessage : AnimatedMessageScene
{
    [Export] private RichTextLabel PlayerName;
    [Export] private TextureRect GamePortrait;

    private string GameName;
    private string MessageText;
    private string Player;

    public override void SetPacket(IMessagePacket packetBase)
    {
        if (packetBase.GetPacket() is not ChatPrintJsonPacket packet) return;
        if (!ConnectionController.HasLeaderClient) return;
        var leader = ConnectionController.LeaderClient!;

        GameName = leader.PlayerGames[packet.Slot];
        MessageText = packet.Message.Sanitize();
        Player = $"{{{{player;{packet.Slot}}}}}";
        
        Reload();
        RunBounceAnimation();
    }

    public override void Reload()
    {
        if (GamePortraitLoader.TryGet(GameName, out var gameImage)) GamePortrait.Texture = gameImage;
        
        Message.Clear();
        PlayerName.Clear();
        
        Message.ApplyCompiledPrintableObjs(MessageText.CompileRichText(GetCompileEffects(), false));
        PlayerName.ApplyCompiledPrintableObjs(Player.CompileRichText(GetCompileEffects(), false));
    }
}