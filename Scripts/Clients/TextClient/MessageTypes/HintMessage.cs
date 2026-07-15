using System.Collections.Generic;
using Archipelago.MultiClient.Net.Packets;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;
using static HydraTextClient.Scripts.Utility.ColorIdConstants;
using static HydraTextClient.Scripts.Utility.ColorIdConstants.ColorConstant;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class HintMessage : MessageScene
{
    public const string SaveId = "Clients/TextClient/HintMessage";
    public const string Default = "{{receiver}}'s {{item}} is at {{loc}} in {{finder}}'s world ({{found}})";

    public override void SetPacket(IMessagePacket packetBase)
    {
        if (packetBase.GetPacket() is not HintPrintJsonPacket hint) return;
        if (!ConnectionController.HasLeaderClient) return;

        CachedReplacement = new Dictionary<string, string>
        {
            ["finder"] = $"{{{{player;{hint.FindingPlayer}}}}}", ["item"] = hint.GetItemEffectText(),
            ["receiver"] = $"{{{{player;{hint.ReceivingPlayer}}}}}", ["loc"] = hint.GetLocationEffectText(),
            ["found"] = hint.GetFoundEffectText(),
        };

        Reload();
    }

    public override void Reload()
    {
        var final = SaveType<string>.Load(SaveId, Default).CompileSimpleText(CachedReplacement);

        UpdateFontSize(Message);
        
        Message.Clear();
        Message.ApplyCompiledPrintableObjs(final.CompileRichText(GetCompileEffects(), false));
    }

    public override bool CanReload(string saveId)
    {
        if (saveId is PlayerConnect) return true;
        if (IdToConstant.TryGetValue(saveId, out var constant))
            return constant.IsPlayerColor() || constant.IsItemColor()
                                            || constant is FoundColor or NotFoundColor or LocationColor;

        return saveId is SaveId;
    }
}