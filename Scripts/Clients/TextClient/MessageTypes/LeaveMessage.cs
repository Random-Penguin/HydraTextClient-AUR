using System.Collections.Generic;
using Archipelago.MultiClient.Net.Packets;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;
using static HydraTextClient.Scripts.Utility.ColorIdConstants;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class LeaveMessage : MessageScene
{
    public const string SaveId = "Clients/TextClient/LeaveMessage";
    public const string Default = "{{player}} left";

    public override void SetPacket(IMessagePacket packetBase)
    {
        if (packetBase.GetPacket() is not LeavePrintJsonPacket packet) return;

        CachedReplacement = new Dictionary<string, string> { ["player"] = $"{{{{player;{packet.Slot}}}}}" };
        Reload();
    }

    public override void Reload()
    {
        var final = SaveType<string>.Load(SaveId, Default).CompileSimpleText(CachedReplacement);

        UpdateFontSize(Message);

        Message.Clear();
        Message.ApplyCompiledPrintableObjs(final.CompileRichText(GetCompileEffects(), false));
    }

    public override bool CanReload(string saveId, out bool queueSelfForDelete)
    {
        queueSelfForDelete = false;
        if (saveId is PlayerConnect) return true;
        if (IdToConstant.TryGetValue(saveId, out var constant)) return constant.IsPlayerColor();
        return saveId is SaveId;
    }
}