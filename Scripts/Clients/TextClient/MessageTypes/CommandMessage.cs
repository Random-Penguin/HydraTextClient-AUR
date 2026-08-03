using Archipelago.MultiClient.Net.Packets;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class CommandMessage : MessageScene
{
    public string Text;

    public override void SetInternalPacket(IMessagePacket packetBase)
    {
        if (packetBase.GetPacket() is not CommandResultPrintJsonPacket result) return;
        Text = result.Data[0].Text;
        Reload();
    }

    public override void Reload()
    {
        UpdateFontSize(TimeStamp);
        UpdateFontSize(Message);
        Message.Text = Text;
    }

    public override string CopyText() => Text;
    public override void RemoveEvents() { }
}