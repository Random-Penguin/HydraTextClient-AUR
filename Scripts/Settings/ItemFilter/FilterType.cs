using System.Runtime.CompilerServices;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using HydraTextClient.Scripts.Controllers;

namespace HydraTextClient.Scripts.Settings.ItemFilter;

public struct FilterType(string itemName, string gameName, ItemFlags itemFlags)
{
    public string ItemName = itemName;
    public string GameName = gameName;
    public ItemFlags ItemFlags = itemFlags;
    public bool ShowInHintsTable = true;
    public bool ShowInItemLog = true;
    public bool IsSpecial = false;
    public string UID => $"{ItemName}%__%{GameName}%__%{ItemFlags}";

    // public static bool operator ==(FilterType ft, ItemInfo item)
    //     => ft.ItemName == item!.ItemName && ft.GameName == item.ItemGame && ft.ItemFlags == item.Flags;
    //
    // public static bool operator !=(FilterType ft, ItemInfo item) => !(ft == item);
    //
    // public static bool operator ==(FilterType ft, Hint hint)
    // {
    //     var leader = ConnectionController.LeaderClient!;
    //     return ft.ItemName == leader.ItemIdToItemName(hint!.ItemId, hint.ReceivingPlayer)
    //            && ft.GameName == leader.PlayerGames[hint.ReceivingPlayer] && ft.ItemFlags == hint.ItemFlags;
    // }
    //
    // public static bool operator !=(FilterType ft, Hint hint) => !(ft == hint);
    //
    // public static bool operator ==(FilterType ft1, FilterType ft2) =>
    //     ft1.ItemName == ft2.ItemName && ft1.GameName == ft2.ItemName && ft1.ItemFlags == ft2.ItemFlags;
    //
    // public static bool operator !=(FilterType ft1, FilterType ft2) => !(ft1 == ft2);
    //
    // public static implicit operator FilterType(ItemInfo item) => new(item.ItemName, item.ItemGame, item.Flags);
    //
    // public static implicit operator FilterType(Hint hint)
    // {
    //     var leader = ConnectionController.LeaderClient!;
    //     return new FilterType(
    //         leader.ItemIdToItemName(hint!.ItemId, hint.ReceivingPlayer), leader.PlayerGames[hint.ReceivingPlayer],
    //         hint.ItemFlags
    //     );
    // }
}

public static class FilterExtensions
{
    extension(ItemInfo itemInfo)
    {
        public string UID => $"{itemInfo.ItemName}%__%{itemInfo.ItemGame}%__%{itemInfo.Flags}";
    }

    extension(Hint hint)
    {
        public string? ItemGame => ConnectionController.HasLeaderClient
            ? ConnectionController.LeaderClient!.PlayerGames[hint.ReceivingPlayer] : null;

        public string? ItemName => ConnectionController.HasLeaderClient
            ? ConnectionController.LeaderClient!.ItemIdToItemName(hint.ItemId, hint.ReceivingPlayer) : null;

        public string UID => $"{hint.ItemName}%__%{hint.ItemGame}%__%{hint.ItemFlags}";
    }
}