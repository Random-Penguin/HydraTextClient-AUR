using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using CreepyUtil.Archipelago.ApClient;
using Godot;
using HydraTextClient.Scripts.Clients.TextClient;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Settings;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.UIHelpers;

namespace HydraTextClient.Scripts.Clients.CircleTracker;

public partial class TrackerPage : Control
{
    private const string ShowEmptyCircles = "circle_tracker/show_empty";
    private ConcurrentQueue<bool> UpdateQueue = [];
    [Export] private PopoutWindow PopoutWindow;
    [Export] private EmptyRichLabelInteractor Label;
    public ConcurrentDictionary<int, ulong[]> Circles = [];
    public ConcurrentDictionary<int, string> CircleItems = [];
    private ApClient Client;
    private int ProcessId;
    private Dictionary<string, Action<RichTextLabel, string[]>>? CompileEffects;
    private IPrintableObj[] CompiledMessage;
    private int TrackedCount;
    private int CurrentCircle;
    private Action<ItemInfo[], int> OnItemsReceived;
    private Action<ReadOnlyCollection<long>> OnLocationsChecked;
    private HydraBridgeEntry Entry;

    [Signal] public delegate void OnStopCalledEventHandler();

    public void Setup(string name, ApClient client, HydraBridgeEntry entry)
    {
        Client = client;
        Name = name;
        PopoutWindow.Title = name;
        Entry = entry;
        
        ProcessId = ExternalAppController.StartProcess(name, entry);

        OnStopCalled += () => ExternalAppController.EndProcess(ProcessId);

        CalculateCircles();
        OnItemsReceived = (_, _) => CallDeferred("CalculateCircles");
        client.ItemHandler.OnNewItemsReceived += OnItemsReceived;
        OnStopCalled += () => client.ItemHandler.OnNewItemsReceived -= OnItemsReceived;

        OnLocationsChecked = _ => QueueUpdate();
        client.CheckedLocationsUpdated += OnLocationsChecked;
        OnStopCalled += () => client.CheckedLocationsUpdated -= OnLocationsChecked;

        SaveType<bool>.OnSaveEvent += (id, _) =>
        {
            if (id is ShowEmptyCircles) QueueUpdate();
        };
    }

    public override void _Process(double delta)
    {
        if (UpdateQueue.IsEmpty) return;
        var recompile = UpdateQueue.Contains(true);
        UpdateQueue.Clear();
        Label.Clear();

        if (recompile)
        {
            StringBuilder sb = new();
            var font = (int)SaveType<double>.Load(GlobalThemeSettings.GlobalFontSize, 20d);
            List<ulong> recordedLocations = [];
            foreach (var (circle, locations) in Circles.OrderBy(kv => kv.Key))
            {
                var uniqueLocations = locations.Except(recordedLocations).ToArray();
                if (uniqueLocations.Length == 0 && !SaveType<bool>.Load(ShowEmptyCircles, true)) continue;

                recordedLocations.AddRange(uniqueLocations);
                uniqueLocations = uniqueLocations.Where(id => Client.MissingRawLocations.Contains((long)id)).ToArray();

                sb.Append("[center][font_size=").Append(font * (uniqueLocations.Length == 0 ? 1 : 2))
                  .Append("]Circle #").Append($"{circle:###,###}").Append("[/font_size]");

                if (uniqueLocations.Length != 0)
                    sb.Append(" (").Append($"{uniqueLocations.Length:###,###}").Append(") locations");

                sb.Append("[/center]\n");

                if (CircleItems[circle].Length != 0)
                    sb.Append("[center]").Append(CircleItems[circle]).Append("[/center]\n");

                foreach (var id in uniqueLocations.OrderBy(id => Client.Locations[(long)id]))
                {
                    sb.Append($" {{{{loc;{id};{Client.PlayerSlot}}}}}\n");
                }
                sb.Append('\n');
            }

            CompiledMessage = sb.ToString().CompileRichText(GetCompileEffects(), true);
        }

        Label.ApplyCompiledPrintableObjs(CompiledMessage);
    }

    public void CalculateCircles()
    {
        var items = Client.ItemHandler.Items.Skip(TrackedCount)
                          .Where(item => (item.Flags.HasFlag(ItemFlags.Advancement)
                                          || item.Flags.HasFlag(ItemFlags.NeverExclude))
                                         && !item.Flags.HasFlag(ItemFlags.Trap)
                           ).ToList();

        if (CurrentCircle is 0)
        {
            CurrentCircle = 1;
            var start = items.TakeWhile(item => item.Player.Name is "Server" && item.LocationName is "Server")
                             .ToArray();
            QueueCircle(CurrentCircle++, start);
            items = items.Skip(TrackedCount).ToList();
        }

        while (items.Count != 0)
        {
            QueueCircle(CurrentCircle++, items[0]);
            items.RemoveAt(0);
        }
    }

    public void QueueCircle(int circle, params ItemInfo[] items)
    {
        CircleItems[circle] = $"{string.Join(", ", items.Select(item => item.GetEffectText()))}";
        Entry.ItemsQueued.Enqueue((circle, items.Select(item => item.ItemId).ToArray()));
        TrackedCount += items.Length;
    }

    public void Stop() => EmitSignalOnStopCalled();
    public void QueueUpdate(bool recompile = true) => UpdateQueue.Enqueue(recompile);

    private Dictionary<string, Action<RichTextLabel, string[]>> GetCompileEffects()
    {
        if (CompileEffects is not null) return CompileEffects;
        return CompileEffects = MessageParser.CreateEffects(() => CallDeferred("UpdateData", false));
    }
}