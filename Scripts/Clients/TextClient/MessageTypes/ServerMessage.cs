using Archipelago.MultiClient.Net.Packets;
using Godot;
using HydraTextClient.Scripts.Controllers;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class ServerMessage : AnimatedMessageScene
{
    [Export] private RichTextLabel PlayerName;
    
    public override void SetPacket(IMessagePacket packetBase)
    {
        if (packetBase.GetPacket() is not ServerChatPrintJsonPacket packet) return;
        if (!ConnectionController.HasLeaderClient) return;

        CompiledMessage = packet.Message.Sanitize().CompileRichText(GetCompileEffects(), false);
        CompiledNameMessage = "{{player;0}}".CompileRichText(GetCompileEffects(), false);

        Reload();
        RunBounceAnimation();
    }

    public override void Reload()
    {
        Message.Clear();
        PlayerName.Clear();
        
        Message.ApplyCompiledPrintableObjs(CompiledMessage);
        PlayerName.ApplyCompiledPrintableObjs(CompiledNameMessage);
    }
}