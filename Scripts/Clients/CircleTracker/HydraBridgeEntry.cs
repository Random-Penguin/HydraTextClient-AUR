using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CreepyUtil.Archipelago.ApClient;
using HydraTextClient.Scripts.Controllers;
using Newtonsoft.Json;

namespace HydraTextClient.Scripts.Clients.CircleTracker;

public class HydraBridgeEntry(string apDir, ApClient client, TrackerPage page)
    : CoreAppEntry($"{apDir}/ArchipelagoLauncherDebug", "HydraUTBridge")
{
    public readonly ConcurrentQueue<(int, long[])> ItemsQueued = [];

    public override void Interactor(string text, StreamWriter input, string console)
    {
        // WriteLine(console, $"Command: [{text}]");

        try
        {
            switch (text)
            {
                case "slot_name": input.WriteLine(client.PlayerName); break;
                case "game": input.WriteLine(client.PlayerGames[client.PlayerSlot]); break;
                case "slot_data": input.WriteLine(JsonConvert.SerializeObject(client.SlotData)); break;
                case "missing_locations":
                    input.WriteLine(string.Join(',', client.Locations.Select(kv => kv.Value))); break;
                case "next":
                    while (ItemsQueued.IsEmpty) Task.Delay(20).Wait();
                    ItemsQueued.TryDequeue(out var next);
                    // WriteLine(console, $"Response: [{next.Item1}],[{string.Join(',', next.Item2)}]");
                    input.WriteLine($"{next.Item1}|{string.Join(',', next.Item2)}");
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
                        page.Circles.TryAdd(circle, ids);
                        page.QueueUpdate();
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