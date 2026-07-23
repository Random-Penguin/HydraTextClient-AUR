using System.Collections.Generic;
using Archipelago.MultiClient.Net.Packets;
using HydraTextClient.Scripts.Clients.TextClient.ParserEffects;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class LeaveMessage : MessageScene
{
    public const string SaveId = "Clients/TextClient/LeaveMessage";
    public const string Default = "{{player}} left";
    public int PlayerSlot;

    public override void SetInternalPacket(IMessagePacket packetBase)
    {
        if (packetBase.GetPacket() is not LeavePrintJsonPacket packet) return;

        CachedReplacement = new Dictionary<string, string>
        {
            ["player"] = $"{{{{player;{PlayerSlot = packet.Slot}}}}}",
        };

        SaveType<string>.AddIndividualEvent(SaveId, CallReload);
        Reload();
    }

    public override void Reload()
    {
        var final = SaveType<string>.Load(SaveId, Default).CompileSimpleText(CachedReplacement);

        UpdateFontSize(Message);

        Message.Clear();
        Message.ApplyCompiledPrintableObjs(final.CompileRichText(GetCompileEffects(), false));
    }

    public override string CopyText() => SaveType<string>.Load(SaveId, Default).CompileSimpleText(
        new Dictionary<string, string> { ["player"] = PlayerEffect.PlayerName(PlayerSlot, out _) }
    );

    public override void RemoveEvents() => SaveType<string>.RemoveIndividualEvent(SaveId, CallReload);
}