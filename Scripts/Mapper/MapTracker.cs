using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using CreepyUtil.Archipelago.ApClient;
using Godot;
using HydraTextClient.Scripts.Clients.CircleTracker;
using HydraTextClient.Scripts.Settings;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.UIHelpers;

namespace HydraTextClient.Scripts.Mapper;

public partial class MapTracker : HSplitContainer
{
    [Export] private Control MapContainer;
    [Export] private Control ButtonContainer;
    [Export] private PackedScene MapScene;
    private Dictionary<string, string> MapPaths = [];
    private Dictionary<string, string> ClientGames = [];
    private ConcurrentDictionary<string, ApClient> Clients = []; // easy access
    private Dictionary<string, ButtonAnimation> Buttons = [];

    public override void _Ready()
    {
        if (!Directory.Exists(Directories.MapPacks)) Directory.CreateDirectory(Directories.MapPacks);
        Reload();

        CircleTracker.CircleTrackerOpened += AddButton;
        CircleTracker.CircleTrackerClosed += RemoveButton;
    }

    public void AddButton(string name, ApClient client)
    {
        if (Buttons.ContainsKey(name)) return;
        ButtonAnimation button = new();
        button.Text = $"{name} ({client.PlayerGame})";
        button.Disabled = !MapPaths.ContainsKey(ClientGames[name] = client.PlayerGame.ToLower().Replace(":", ""));
        button.Pressed += () => button.Disabled = LoadMap(ClientGames[name], button.Text, name, client);
        Buttons.Add(name, button);
        Clients.TryAdd(name, client);
        ButtonContainer.CallDeferred("add_child", button);
    }

    public void RemoveButton(string name, ApClient __)
    {
        Clients.Remove(name, out _);
        ClientGames.Remove(name);
        if (Buttons.Remove(name, out var button)) ButtonContainer.CallDeferred("remove_child", button);
        // if (Pages.Remove(name, out var page))
        // {
        // 	page.Stop();
        // 	PageContainer.CallDeferred("remove_child", page);
        // }
    }

    public bool LoadMap(string game, string tabName, string trackerName, ApClient client)
    {
        var map = MapScene.Instantiate<MapLoader>();
        map.Client = client;
        map.Name = tabName;
        map.CallDeferred("Setup", MapPaths[game], trackerName);
        MapContainer.CallDeferred("add_child", map);
        return true;
    }

    public void Reload()
    {
        MapPaths.Clear();
        foreach (var game in Directory.GetDirectories(Directories.MapPacks))
        {
            var gameName = Path.GetFileName(game);
            MapPaths[gameName.ToLower().Replace(":", "")] = game;
        }
    }
}