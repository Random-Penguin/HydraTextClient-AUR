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