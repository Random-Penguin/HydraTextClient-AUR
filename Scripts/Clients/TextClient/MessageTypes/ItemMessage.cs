using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using Godot;
using HydraTextClient.Scripts.Clients.TextClient.ParserEffects;
using HydraTextClient.Scripts.Connection.Slots;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Settings.ItemFilter;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;
using static HydraTextClient.Scripts.Clients.TextClient.TextClient;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class ItemMessage : MessageScene
{
    public const string SaveIdSamePerson = "Clients/TextClient/ItemMessageSamePerson";
    public const string DefaultSamePerson = "{{finder}} found their {{item}} at {{loc}}";

    public const string SaveIdDifferentPerson = "Clients/TextClient/ItemMessageDifferentPerson";
    public const string DefaultDifferentPerson = "{{finder}} found {{item}} for {{receiver}} at {{loc}}";
    private bool FinderIsReceiver;
    private Action ReloadAction;
    private ItemFlags Flags;
    private string FinderName;
    private string ReceiverName;
    private int FinderSlot;
    private int ReceiverSlot;
    public string ItemName;
    public string LocationName;
    private FilterType ThisFilter;

    public override void SetInternalPacket(IMessagePacket packetBase)
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
        ThisFilter = new FilterType(item.ItemName, item.ItemGame, Flags);

        CachedReplacement = new Dictionary<string, string>
        {
            ["finder"] = $"{{{{player;{item.FindingPlayer}}}}}", ["item"] = item.GetItemEffectText(),
            ["receiver"] = $"{{{{player;{item.ReceivingPlayer}}}}}", ["loc"] = item.GetLocationEffectText(),
        };

        SaveType<bool>.AddIndividualEvents(ReloadVisibility, ShowOnlyYou, GetFlagSaveIdKey());
        SaveType<string>.AddIndividualEvents(CallReload, SaveIdSamePerson, SaveIdDifferentPerson);
        SaveType<FilterType>.AddIndividualEvent(ThisFilter.UID, ReloadFilter);
        CallReload();
    }

    public string GetFlagSaveIdKey() => Flags.HasFlag(ItemFlags.Advancement) ? ShowProgressive
        : Flags.HasFlag(ItemFlags.NeverExclude) ? ShowUseful : Flags.HasFlag(ItemFlags.Trap) ? ShowTrap : ShowNormal;

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

    public void ReloadVisibility(bool _)
    {
        var isAdvancement = Flags.HasFlag(ItemFlags.Advancement);
        var isNeverExclude = Flags.HasFlag(ItemFlags.NeverExclude) && !Flags.HasFlag(ItemFlags.Advancement);
        var isTrap = Flags.HasFlag(ItemFlags.Trap);
        var isNormal = Flags is ItemFlags.None;

        var showAdvancement = SaveType<bool>.Load(ShowProgressive, true) && isAdvancement;
        var showNeverExclude = SaveType<bool>.Load(ShowUseful, true) && isNeverExclude;
        var showTrap = SaveType<bool>.Load(ShowTrap, true) && isTrap;
        var showNormal = SaveType<bool>.Load(ShowNormal, true) && isNormal;

        Visible = showAdvancement || showNeverExclude || showTrap || showNormal;
        if (!Visible) return;
        var isRelatedToYou = SlotView.ContainsSlot(FinderName) || SlotView.ContainsSlot(ReceiverName);
        var showRelatedToYou = SaveType<bool>.Load(ShowOnlyYou, false);
        if (showRelatedToYou && !isRelatedToYou
            || SaveType<FilterType>.TryGet(ThisFilter.UID, out var filter) && !filter.ShowInItemLog) Visible = false;
        if (Visible) CallReload();
    }

    public void ReloadFilter(FilterType _) => ReloadVisibility(true);

    public override string CopyText() => SaveType<string>.Load(
        FinderIsReceiver ? SaveIdSamePerson : SaveIdDifferentPerson,
        FinderIsReceiver ? DefaultSamePerson : DefaultDifferentPerson
    ).CompileSimpleText(
        new Dictionary<string, string>
        {
            ["finder"] = PlayerEffect.PlayerName(FinderSlot, out _), ["item"] = ItemName,
            ["receiver"] = PlayerEffect.PlayerName(ReceiverSlot, out _), ["loc"] = LocationName,
        }
    );

    public override void RemoveEvents()
    {
        SaveType<bool>.RemoveIndividualEvents(ReloadVisibility, ShowOnlyYou, GetFlagSaveIdKey());
        SaveType<string>.RemoveIndividualEvents(CallReload, SaveIdSamePerson, SaveIdDifferentPerson);
        SaveType<FilterType>.RemoveIndividualEvent(ThisFilter.UID, ReloadFilter);
    }
}