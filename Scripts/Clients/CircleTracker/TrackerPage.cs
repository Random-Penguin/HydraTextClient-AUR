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
using HydraTextClient.Scripts.Settings.ItemFilter;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.UIHelpers;

namespace HydraTextClient.Scripts.Clients.CircleTracker;

public partial class TrackerPage : Control
{
    private const string ShowEmptyCircles = "circle_tracker/show_empty";
    private const string ShowFutureCircles = "circle_tracker/spoil_future";
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
    private Action<Hint[]> OnHintsUpdated;
    private Action<string, bool> OnBoolSaveDataUpdated;
    private Action<string, FilterType> OnFilterDataUpdated;
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

        OnLocationsChecked = _ => QueueUpdate();
        client.CheckedLocationsUpdated += OnLocationsChecked;

        OnHintsUpdated = _ => QueueUpdate();
        client.HintsTrackedEvent += OnHintsUpdated;

        OnBoolSaveDataUpdated = (id, _) =>
        {
            if (id is ShowEmptyCircles or ShowFutureCircles) QueueUpdate();
        };
        SaveType<bool>.OnSaveEvent += OnBoolSaveDataUpdated;
        
        OnFilterDataUpdated = (_, _) => QueueUpdate();
        SaveType<FilterType>.OnSaveEvent += OnFilterDataUpdated;
        OnStopCalled += () =>
        {
            client.CheckedLocationsUpdated -= OnLocationsChecked;
            client.HintsTrackedEvent -= OnHintsUpdated;
            client.ItemHandler.OnNewItemsReceived -= OnItemsReceived;
            SaveType<bool>.OnSaveEvent -= OnBoolSaveDataUpdated;
            SaveType<FilterType>.OnSaveEvent -= OnFilterDataUpdated;
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
            var localHints = Client.Hints.Where(hint => hint.FindingPlayer == Client.PlayerSlot).ToArray();
            var hints = localHints.ToDictionary(hint => hint.LocationId, hint => hint.GetItemEffectText());
            var priority = localHints.Where(hint => hint.Status is HintStatus.Priority).Select(hint => hint.LocationId)
                                     .ToArray();
            var firstEnd = SaveType<bool>.Load(ShowFutureCircles, false);
            foreach (var (circle, locations) in Circles.OrderBy(kv => kv.Key))
            {
                var uniqueLocations = locations.Except(recordedLocations).ToArray();
                recordedLocations.AddRange(uniqueLocations);
                uniqueLocations = uniqueLocations.Where(id => Client.MissingRawLocations.Contains((long)id)).ToArray();

                if (uniqueLocations.Length == 0 && !SaveType<bool>.Load(ShowEmptyCircles, true)) continue;

                sb.Append("[center][font_size=").Append(font * (uniqueLocations.Length == 0 ? 1 : 2))
                  .Append("]Circle #").Append($"{circle:###,###}").Append("[/font_size]");

                if (uniqueLocations.Length != 0)
                    sb.Append(" (").Append($"{uniqueLocations.Length:###,###}").Append(") locations");

                sb.Append("[/center]\n");

                if (CircleItems[circle].Length != 0)
                    sb.Append("[center]").Append(CircleItems[circle]).Append("[/center]\n");
                
                if (uniqueLocations.Length == 0) continue;
                var orderedLocations = uniqueLocations
                                      .OrderByDescending(id => priority.Contains((long)id))
                                      .ThenBy(id => Client.Locations[(long)id]).ToArray();

                sb.Append("[table=2][cell bg=#00000069] Locations [/cell][cell bg=#00000069] Hinted Items [/cell]");
                for (var i = 0; i < orderedLocations.Length; i++)
                {
                    var id = orderedLocations[i];
                    sb.Append(i % 2 == 0 ? "[cell bg=#00000044]" : "[cell]").Append(" {{loc;").Append(id).Append(';')
                      .Append(Client.PlayerSlot).Append("}}[/cell]").Append(i % 2 == 0 ? "[cell bg=#00000044] " : "[cell] ");
                    if (hints.TryGetValue((long)id, out var item)) sb.Append(item);
                    sb.Append(" [/cell]");
                }
                sb.Append("[/table]\n");
                if (!firstEnd) break;
            }

            if (sb.ToString().Trim() is "") sb.Append("Super BK :(\nEither that or there was an error from UT");
            CompiledMessage = sb.ToString().CompileRichText(GetCompileEffects(), true);
        }

        Label.ApplyCompiledPrintableObjs(CompiledMessage);
    }

    public void CalculateCircles()
    {
        var items = Client.ItemHandler.Items
                          .Where(item => item.Player.Name is "Server"
                                         || (item.Flags.HasFlag(ItemFlags.Advancement)
                                             || item.Flags.HasFlag(ItemFlags.NeverExclude))
                                         && !item.Flags.HasFlag(ItemFlags.Trap)
                           ).ToArray();

        if (CurrentCircle is 0)
        {
            CurrentCircle = 1;
            var start = items.TakeWhile(item => item.Player.Name is "Server" && item.LocationName is "Server")
                             .ToArray();
            QueueCircle(CurrentCircle++, start);
        }

        while (items.Length > TrackedCount) { QueueCircle(CurrentCircle++, items.Take(TrackedCount + 1).ToArray()); }
    }

    public void QueueCircle(int circle, params ItemInfo[] items)
    {
        CircleItems[circle] = $"{string.Join(", ", items.Skip(TrackedCount).Select(item => item.GetEffectText()))}";
        Entry.ItemsQueued.Enqueue((circle, items.Select(item => item.ItemId).ToArray()));
        TrackedCount = items.Length;
    }

    public void Stop() => EmitSignalOnStopCalled();
    public void QueueUpdate(bool recompile = true) => UpdateQueue.Enqueue(recompile);

    private Dictionary<string, Action<RichTextLabel, string[]>> GetCompileEffects()
    {
        if (CompileEffects is not null) return CompileEffects;
        return CompileEffects = MessageParser.CreateEffects(() => CallDeferred("UpdateData", false));
    }

    public void Failure(string text) => Label.Text = $"[color=red]{text}[/color]";
}