using System.Collections.Generic;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using HydraTextClient.Scripts.Clients.TextClient.ParserEffects;
using HydraTextClient.Scripts.Connection.Slots;
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
    private ItemFlags Flags;
    private string FinderName;
    private string ReceiverName;
    private int FinderSlot;
    private int ReceiverSlot;
    public string ItemName;
    public string LocationName;

    public override void SetPacket(IMessagePacket packetBase)
    {
        if (packetBase.GetPacket() is not ItemPrintJsonPacket item) return;
        if (!ConnectionController.HasLeaderClient) return;

        var leader = ConnectionController.LeaderClient!;
        FinderIsReceiver = item.FinderIsReceiver;
        Flags = item.Item.Flags;
        FinderName = leader.PlayerNames[FinderSlot = item.FindingPlayer];
        ReceiverName = leader.PlayerNames[ReceiverSlot = item.ReceivingPlayer];
        ItemName = item.ItemName;
        LocationName = item.GetLocationName();

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

        UpdateFontSize(Message);

        Message.Clear();
        Message.ApplyCompiledPrintableObjs(final.CompileRichText(GetCompileEffects(), false));
    }

    public override bool CanReload(string saveId, out bool queueSelfForDelete)
    {
        queueSelfForDelete = false;
        if (saveId is PlayerConnect) return true;
        if (IdToConstant.TryGetValue(saveId, out var constant))
            return constant.IsPlayerColor() || constant.IsItemColor() || constant is LocationColor;

        switch (saveId)
        {
            case TextClient.ShowProgressive:
                if (SaveType<bool>.Load(saveId, true) && Flags.HasFlag(ItemFlags.Advancement))
                    queueSelfForDelete = true;
                break;
            case TextClient.ShowUseful:
                if (SaveType<bool>.Load(saveId, true) && Flags.HasFlag(ItemFlags.NeverExclude))
                    queueSelfForDelete = true;
                break;
            case TextClient.ShowNormal:
                if (SaveType<bool>.Load(saveId, true) && Flags.HasFlag(ItemFlags.None)) queueSelfForDelete = true;
                break;
            case TextClient.ShowTrap:
                if (SaveType<bool>.Load(saveId, true) && Flags.HasFlag(ItemFlags.Trap)) queueSelfForDelete = true;
                break;
            case TextClient.ShowOnlyYou:
                if (SaveType<bool>.Load(saveId, false) && !SlotView.ContainsSlot(FinderName)
                                                       && !SlotView.ContainsSlot(ReceiverName))
                    queueSelfForDelete = true;
                break;
            case SaveIdDifferentPerson or SaveIdSamePerson: return true;
        }

        return false;
    }

    public override string CopyText() => SaveType<string>.Load(
        FinderIsReceiver ? SaveIdSamePerson : SaveIdDifferentPerson,
        FinderIsReceiver ? DefaultSamePerson : DefaultDifferentPerson
    ).CompileSimpleText(new Dictionary<string, string>
    {
        ["finder"] = PlayerEffect.PlayerName(FinderSlot, out _), ["item"] = ItemName,
        ["receiver"] = PlayerEffect.PlayerName(ReceiverSlot, out _), ["loc"] = LocationName,
    });
}