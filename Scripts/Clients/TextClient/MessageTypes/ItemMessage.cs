using System.Collections.Generic;
using Archipelago.MultiClient.Net.Packets;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;
using static HydraTextClient.Scripts.Utility.ColorIdConstants;
using static HydraTextClient.Scripts.Utility.ColorIdConstants.ColorConstant;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class ItemMessage : MessageScene
{
    public const string SaveIdSamePerson = "Clients/TextClient/ItemMessageSamePerson";
    public const string DefaultSamePerson = "{{finder}} found their {{item}} at {{loc}}";

    public const string SaveIdDifferentPerson = "Clients/TextClient/ItemMessageDifferentPerson";
    public const string DefaultDifferentPerson = "{{finder}} found {{item}} for {{receiver}} at {{loc}}";
    private bool FinderIsReceiver;

    public override void SetPacket(IMessagePacket packetBase)
    {
        if (packetBase.GetPacket() is not ItemPrintJsonPacket item) return;
        if (!ConnectionController.HasLeaderClient) return;

        FinderIsReceiver = item.FinderIsReceiver;
        CachedReplacement = new Dictionary<string, string>
        {
            ["finder"] = $"{{{{player;{item.FindingPlayer}}}}}", ["item"] = item.GetItemEffectText(),
            ["receiver"] = $"{{{{player;{item.ReceivingPlayer}}}}}", ["loc"] = item.GetLocationEffectText(),
        };

        Reload();
    }

    public override void Reload()
    {
        var final = SaveType<string>.Load(
            FinderIsReceiver ? SaveIdSamePerson : SaveIdDifferentPerson,
            FinderIsReceiver ? DefaultSamePerson : DefaultDifferentPerson
        ).CompileSimpleText(CachedReplacement);

        Message.Clear();
        Message.ApplyCompiledPrintableObjs(final.CompileRichText(GetCompileEffects(), false));
    }

    public override bool CanReload(string saveId)
    {
        if (saveId is PlayerConnect) return true;
        if (IdToConstant.TryGetValue(saveId, out var constant))
            return constant.IsPlayerColor() || constant.IsItemColor() || constant is LocationColor;

        return saveId is SaveIdDifferentPerson or SaveIdSamePerson;
    }
}