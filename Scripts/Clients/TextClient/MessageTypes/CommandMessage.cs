using Archipelago.MultiClient.Net.Packets;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class CommandMessage : MessageScene
{
    public string Text;

    public override void SetPacket(IMessagePacket packetBase)
    {
        if (packetBase.GetPacket() is not CommandResultPrintJsonPacket result) return;
        Text = result.Data[0].Text;
        Reload();
    }

    public override void Reload() => Message.Text = Text;
    public override bool CanReload(string saveId) => false;
}