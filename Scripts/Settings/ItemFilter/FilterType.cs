using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using HydraTextClient.Scripts.Controllers;
using static HydraTextClient.Scripts.Settings.ItemFilter.FilterType;

namespace HydraTextClient.Scripts.Settings.ItemFilter;

public struct FilterType(string itemName, string gameName, ItemFlags itemFlags)
{
    public string ItemName = itemName;
    public string GameName = gameName;
    public ItemFlags ItemFlags = itemFlags;
    public bool ShowInHintsTable = true;
    public bool ShowInItemLog = true;
    public bool IsSpecial = false;
    public string UID => MakeUID(ItemName, GameName, ItemFlags);

    public static string MakeUID(string itemName, string gameName, ItemFlags itemFlags)
        => $"{itemName}%__%{gameName}%__%{(int)itemFlags}";
}

public static class FilterExtensions
{
    extension(ItemInfo itemInfo)
    {
        public string UID => MakeUID(itemInfo.ItemName, itemInfo.ItemGame, itemInfo.Flags);
    }

    extension(Hint hint)
    {
        public string? ItemGame => ConnectionController.HasLeaderClient
            ? ConnectionController.LeaderClient!.PlayerGames[hint.ReceivingPlayer] : null;

        public string? ItemName => ConnectionController.HasLeaderClient
            ? ConnectionController.LeaderClient!.ItemIdToItemName(hint.ItemId, hint.ReceivingPlayer) : null;
        
        public string? LocationName => ConnectionController.HasLeaderClient
            ? ConnectionController.LeaderClient!.LocationIdToLocationName(hint.LocationId, hint.FindingPlayer) : null;
        
        public string UID => MakeUID(hint.ItemName, hint.ItemGame, hint.ItemFlags);

        public string EntranceName => hint.Entrance == "" ? "Vanilla" : hint.Entrance;
    }

    extension(ItemPrintJsonPacket item)
    {
        public string UID
        {
            get
            {
                var leader = ConnectionController.LeaderClient!;
                var receiver = item.ReceivingPlayer;
                return MakeUID(
                    leader.ItemIdToItemName(item.Item.Item, receiver), leader.PlayerGames[receiver], item.Item.Flags
                );
            }
        }
    }
}