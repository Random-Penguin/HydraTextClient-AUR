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
    private const string HydraUTBridgeFileHash = "";
    private ConcurrentQueue<bool> UpdateQueue = [];
    [Export] private PopoutWindow PopoutWindow;
    [Export] private EmptyRichLabelInteractor Label;
    public ConcurrentDictionary<int, ulong[]> Circles = [];
    private ApClient Client;
    private int ProcessId;
    private Dictionary<string, Action<RichTextLabel, string[]>>? CompileEffects;
    private IPrintableObj[] CompiledMessage;
    private int TrackedCount;
    private Action<ItemInfo[], int> OnItemsReceived;
    private Action<ReadOnlyCollection<long>> OnLocationsChecked;

    [Signal] public delegate void OnStopCalledEventHandler();

    public void Setup(string name, ApClient client, HydraBridgeEntry entry)
    {
        Client = client;
        Name = name;
        PopoutWindow.Title = name;

        ProcessId = ExternalAppController.StartProcess(name, entry, HydraUTBridgeFileHash);
        OnStopCalled += () => ExternalAppController.EndProcess(ProcessId);

        var start = client.ItemHandler.Items
                          .TakeWhile(item => item.Player.Name is "Server" && item.LocationName is "Server").Count();
        var from = client.ItemHandler.GetItemsFrom(start).ToArray();
        TrackedCount = start + from.Length;
        foreach (var item in from.Where(item => !item.Flags.HasFlag(ItemFlags.Trap)
                                                && !item.Flags.HasFlag(ItemFlags.None)
                 )) entry.ItemsReceived.Enqueue(item.ItemId);

        OnItemsReceived = (_, _) =>
        {
            try
            {
                var newInfos = client.ItemHandler.GetItemsFrom(TrackedCount).ToArray();
                TrackedCount += newInfos.Length;
                foreach (var item in newInfos.Where(item => !item.Flags.HasFlag(ItemFlags.Trap)
                                                            && !item.Flags.HasFlag(ItemFlags.None)
                         )) entry.ItemsReceived.Enqueue(item.ItemId);
            }
            catch (Exception e) { GD.PrintErr(e); }
        };

        client.ItemHandler.OnNewItemsReceived += OnItemsReceived;
        OnStopCalled += () => client.ItemHandler.OnNewItemsReceived -= OnItemsReceived;

        OnLocationsChecked = _ => QueueUpdate();
        client.CheckedLocationsUpdated += OnLocationsChecked;
        OnStopCalled += () => client.CheckedLocationsUpdated -= OnLocationsChecked;
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
            foreach (var (circle, locations) in Circles.OrderBy(kv => kv.Key))
            {
                sb.Append("[center][font_size=").Append(font * 2).Append("]Circle #").Append($"{circle:###,###}")
                  .Append("[/font_size][/center]\n");
                foreach (var id in locations)
                {
                    if (!Client.MissingLocations.Contains(Client.Locations[(long)id])) continue;
                    sb.Append($" {{{{loc;{id};{Client.PlayerSlot}}}}}\n");
                }
                sb.Append('\n');
            }

            CompiledMessage = sb.ToString().CompileRichText(GetCompileEffects(), true);
        }

        Label.ApplyCompiledPrintableObjs(CompiledMessage);
    }

    public void Stop() => EmitSignalOnStopCalled();
    public void QueueUpdate(bool recompile = true) => UpdateQueue.Enqueue(recompile);

    private Dictionary<string, Action<RichTextLabel, string[]>> GetCompileEffects()
    {
        if (CompileEffects is not null) return CompileEffects;
        return CompileEffects = MessageParser.CreateEffects(() => CallDeferred("UpdateData", false));
    }
}