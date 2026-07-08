using System.Collections.Generic;
using CreepyUtil.DiscordRpc;
using Godot;
using HydraTextClient.Scripts.Controllers;
using static HydraTextClient.Scripts.Discord.DRPC_GTI;
using static HydraTextClient.Scripts.Discord.DRPC_GTT;

namespace HydraTextClient.Scripts.Discord;

public static class DRPC
{
    private const string AppId = "1339447230909644851"; 
    public static Dictionary<string, string> LastLocationChecked = [];

    public static void Init()
    {
        ConnectionController.OnClientLeaderChanged += (_, _) => DiscordIntegration.UpdateActivity();
        ConnectionController.OnClientConnection += (_, client, _) =>
        {
            client.OnItemLogPacketReceived += packet =>
            {
                var player = packet.Item.Player;
                LastLocationChecked[client.PlayerNames[player]] = client.LocationIdToLocationName(
                    packet.Item.Location, player
                );
                if (client.PlayerSlot == player) DiscordIntegration.UpdateActivity();
            };
        };
        ConnectionController.OnFullDisconnection += () => LastLocationChecked.Clear();

        DiscordIntegration.LogOut = GD.Print;
        DiscordIntegration.Details = () =>
        {
            if (!ConnectionController.HasLeaderClient) return "Not connected";
            var leader = ConnectionController.LeaderClient!;
            var game = leader.PlayerGames[leader.PlayerSlot];
            return GameToTitle.GetValueOrDefault(game, game);
        };

        DiscordIntegration.State = () =>
        {
            if (!ConnectionController.HasLeaderClient) return "";
            var leader = ConnectionController.LeaderClient!;
            return $"In the Multiworld ({leader.LocationsCheckedCount} / {leader.LocationCount})";
        };

        DiscordIntegration.LargeImage = () =>
        {
            if (!ConnectionController.HasLeaderClient) return "archipelago";
            var leader = ConnectionController.LeaderClient!;
            var game = leader.PlayerGames[leader.PlayerSlot];
            return GameToImage.GetValueOrDefault(game, "archipelago");
        };

        DiscordIntegration.LargeText = () =>
        {
            if (!ConnectionController.HasLeaderClient) return "Not Connected";
            return LastLocationChecked.TryGetValue(ConnectionController.LeaderClient!.PlayerName, out var loc)
                ? $"Last Location Checked: {loc}" : "Nothing Yet";
        };

        DiscordIntegration.SmallImage = () => "archipelago";

        DiscordIntegration.SmallText = () => !ConnectionController.HasLeaderClient ? "Not connected"
            : $"{ConnectionController.LeaderClient!.PlayerNames.Length - 1} Player Multiworld";
        
        DiscordIntegration.InitDiscord(AppId);
        CheckDiscord();
    }

    public static void CheckDiscord() => DiscordIntegration.UpdateActivity();
}