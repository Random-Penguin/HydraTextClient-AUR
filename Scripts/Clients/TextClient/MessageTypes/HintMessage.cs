using System.Collections.Generic;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using HydraTextClient.Scripts.Clients.TextClient.ParserEffects;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Hints;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;
using static HydraTextClient.Scripts.Utility.ColorIdConstants;
using static HydraTextClient.Scripts.Utility.ColorIdConstants.ColorConstant;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class HintMessage : MessageScene
{
    public const string SaveId = "Clients/TextClient/HintMessage";
    public const string Default = "{{receiver}}'s {{item}} is at {{loc}} in {{finder}}'s world ({{found}})";
    private bool HasBeenFound;
    public ItemFlags Flags;
    private int FinderSlot;
    private int ReceiverSlot;
    public string ItemName;
    public string LocationName;
    public bool IsFound;

    public override void SetPacket(IMessagePacket packetBase)
    {
        if (packetBase.GetPacket() is not HintPrintJsonPacket hint) return;
        if (!ConnectionController.HasLeaderClient) return;
        Flags = hint.Item.Flags;
        ItemName = hint.ItemName;
        LocationName = hint.GetLocationName();

        HasBeenFound = hint.Found!.Value;
        CachedReplacement = new Dictionary<string, string>
        {
            ["finder"] = $"{{{{player;{FinderSlot = hint.FindingPlayer}}}}}", ["item"] = hint.GetItemEffectText(),
            ["receiver"] = $"{{{{player;{ReceiverSlot = hint.ReceivingPlayer}}}}}",
            ["loc"] = hint.GetLocationEffectText(), ["found"] = hint.GetFoundEffectText(),
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

    public override bool CanReload(string saveId, out bool queueSelfForDelete)
    {
        queueSelfForDelete = false;
        if (saveId is PlayerConnect) return true;
        if (IdToConstant.TryGetValue(saveId, out var constant))
            return constant.IsPlayerColor() || constant.IsItemColor()
                                            || constant is FoundColor or NotFoundColor or LocationColor;

        if (saveId is TextClient.ShowFoundHints && !SaveType<bool>.Load(saveId, true) && HasBeenFound)
            queueSelfForDelete = true;

        return saveId is SaveId;
    }

    public override string CopyText() => SaveType<string>.Load(
        Flags.HasFlag(ItemFlags.Advancement) ? HintTable.GlobalCopyFormatProgressive : HintTable.GlobalCopyFormat,
        "{{receiver}}'s __{{item}}__ is in `{{finder}}`'s world at **{{loc}}**\\n-# {{entrance}}"
    ).CompileSimpleText(
        new Dictionary<string, string>
        {
            ["finder"] = PlayerEffect.PlayerName(FinderSlot, out _),
            ["receiver"] = PlayerEffect.PlayerName(ReceiverSlot, out _), ["loc"] = LocationName,
            ["entrance"] = HasBeenFound ? "(Found)" : "(Not Found)", ["item"] = ItemName,
        }
    );
}