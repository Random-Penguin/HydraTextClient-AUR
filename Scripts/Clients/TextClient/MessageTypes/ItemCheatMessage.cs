using System.Collections.Generic;
using Archipelago.MultiClient.Net.Packets;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;
using static HydraTextClient.Scripts.Utility.ColorIdConstants;
using static HydraTextClient.Scripts.Utility.ColorIdConstants.ColorConstant;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class ItemCheatMessage : MessageScene
{
    public const string SaveId = "Clients/TextClient/ItemCheatMessage";
    public const string Default = "{{player}} was given {{item}} from {{server}} {{loc}}";

    public override void SetPacket(IMessagePacket packetBase)
    {
        if (packetBase.GetPacket() is not ItemCheatPrintJsonPacket item) return;

        CachedReplacement = new Dictionary<string, string>
        {
            ["player"] = $"{{{{player;{item.FindingPlayer}}}}}", ["server"] = "{{player;0}}",
            ["loc"] = item.GetLocationEffectText(), ["item"] = item.GetItemEffectText(),
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
            return constant.IsPlayerColor() || constant.IsItemColor() || constant is LocationColor;

        return saveId is SaveId;
    }
}