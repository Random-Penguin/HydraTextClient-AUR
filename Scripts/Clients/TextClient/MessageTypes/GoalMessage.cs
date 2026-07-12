using System.Collections.Generic;
using Archipelago.MultiClient.Net.Packets;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;
using static HydraTextClient.Scripts.Utility.ColorIdConstants;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class GoalMessage : MessageScene
{
    public const string SaveId = "Clients/TextClient/GoalMessage";
    public const string Default = "{{player}} Goaled!";
    
    public override void SetPacket(IMessagePacket packetBase)
    {
        if (packetBase.GetPacket() is not GoalPrintJsonPacket packet) return;

        CachedReplacement = new Dictionary<string, string> { ["player"] = $"{{{{player;{packet.Slot}}}}}", };

        Reload();
    }

    public override void Reload()
    {
        var final = SaveType<string>.Load(SaveId, Default).CompileSimpleText(CachedReplacement);

        Message.Clear();
        Message.ApplyCompiledPrintableObjs(final.CompileRichText(GetCompileEffects(), false));
    }

    public override bool CanReload(string saveId)
    {
        if (saveId is PlayerConnect) return true;
        if (IdToConstant.TryGetValue(saveId, out var constant)) return constant.IsPlayerColor();
        return saveId is SaveId;
    }
}