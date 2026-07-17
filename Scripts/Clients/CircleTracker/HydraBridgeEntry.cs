using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CreepyUtil.Archipelago.ApClient;
using HydraTextClient.Scripts.Controllers;
using Newtonsoft.Json;

namespace HydraTextClient.Scripts.Clients.CircleTracker;

public class HydraBridgeEntry : CoreAppEntry
{
    private readonly ApClient Client;
    private readonly TrackerPage Page;
    private readonly long[] StartingItems;
    public readonly ConcurrentQueue<long> ItemsReceived = [];

    public HydraBridgeEntry(string apDir, ApClient client, TrackerPage page) : base(
        $"{apDir}/ArchipelagoLauncherDebug", "HydraUTBridge"
    )
    {
        Client = client;
        Page = page;
        StartingItems = client.ItemHandler.Items
                              .TakeWhile(item => item.Player.Name is "Server" && item.LocationName is "Server")
                              .Select(item => item.ItemId).ToArray();
    }

    public override void Interactor(string text, StreamWriter input, string console)
    {
        WriteLine(console, $"Command: [{text}]");

        try
        {
            switch (text)
            {
                case "slot_name": input.WriteLine(Client.PlayerName); break;
                case "game": input.WriteLine(Client.PlayerGames[Client.PlayerSlot]); break;
                case "slot_data": input.WriteLine(JsonConvert.SerializeObject(Client.SlotData)); break;
                case "missing_locations":
                    input.WriteLine(string.Join(',', Client.MissingLocations.Select(s => Client.Locations[s]))); break;
                case "circle": input.WriteLine("1"); break;
                case "starting_items": input.WriteLine(string.Join(',', StartingItems)); break;
                case "next":
                    while (ItemsReceived.IsEmpty) Task.Delay(20).Wait();
                    ItemsReceived.TryDequeue(out var nextItem);
                    input.WriteLine(nextItem);
                    break;

                default:
                    if (text.StartsWith("exit")) return;
                    if (text.StartsWith("ERROR: "))
                    {
                        WriteError(console, text);
                        MainController.ShowError(text);
                        return;
                    }
                    if (text.StartsWith("Circle "))
                    {
                        var split = text.Split('|');
                        var circle = int.Parse(split[0].Replace("Circle ", ""));
                        var remaining = split[1][1..^1]; 
                        if (remaining.Trim().Length is 0) return;
                        var ids = remaining.Split(',').Select(id => ulong.Parse(id.Trim())).ToArray();
                        Page.Circles.TryAdd(circle, ids);
                        Page.QueueUpdate();
                    }
                    break;
            }
        }
        catch (Exception e)
        {
            WriteError(console, $"Error with [{text}]", e);
            Task.Delay(120).Wait();
        }
    }
}