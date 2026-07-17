using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using CreepyUtil.Archipelago.ApClient;
using Godot;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Settings;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Clients.CircleTracker;

public partial class CircleTracker : Control
{
    [Export] private PackedScene TrackerScene;
    [Export] private VBoxContainer ButtonContainer;
    [Export] private TabContainer PageContainer;

    public Dictionary<string, Button> Buttons = [];
    public Dictionary<string, TrackerPage> Pages = [];
    public ConcurrentDictionary<string, ApClient> Clients = []; // easy access

    public override void _Ready()
    {
        ConnectionController.OnClientConnection += (name, client, _) => AddButton(name, client);
        ConnectionController.OnClientRemoved += (name, _, _) => RemoveButton(name);
    }

    public void AddButton(string name, ApClient client)
    {
        Button button = new();
        button.Pressed += () =>
        {
            button.Disabled = true;
            OpenTracker(name);
        };
        button.Text = name;
        Buttons.Add(name, button);
        Clients.TryAdd(name, client);
        ButtonContainer.CallDeferred("add_child", button);
    }

    public void RemoveButton(string name)
    {
        Clients.Remove(name, out _);
        if (Buttons.Remove(name, out var button)) ButtonContainer.CallDeferred("remove_child", button);
        if (Pages.Remove(name, out var page))
        {
            page.Stop();
            PageContainer.CallDeferred("remove_child", page);
        }
    }

    public void OpenTracker(string name)
    {
        var apDir = SaveType<string>.Load(GlobalThemeSettings.ApDir, "");
        if (apDir is "" || !Directory.Exists(apDir))
        {
            MainController.ShowError(
                apDir is "" ? "Archipelago Directory not set, set it in the Settings/Theme"
                    : "Invalid Archipelago Directory"
            );
            return;
        }

        if (!DoesApWorldExist(apDir, "HydraUTBridge")) return;
        if (!DoesApWorldExist(apDir, "tracker")) return;

        var page = TrackerScene.Instantiate<TrackerPage>();
        HydraBridgeEntry entry;
        try { entry = new HydraBridgeEntry(apDir, Clients[name], page); }
        catch (Exception e)
        {
            MainController.ShowError($"Error with [{apDir}]", e);
            page.QueueFree();
            return;
        }

        if (!entry.FileExists())
        {
            MainController.ShowError(
                "The ArchipelagoLauncherDebug executable was not found in your Archipelago Folder"
            );
            page.QueueFree();
            return;
        }

        page.OnStopCalled += () =>
        {
            if (Pages.Remove(name, out var node)) PageContainer.CallDeferred("remove_child", node);
            Buttons[name].Disabled = false;
        };
        page.Setup(name, Clients[name], entry);
        Pages.Add(name, page);
        PageContainer.CallDeferred("add_child", page);
    }

    public bool DoesApWorldExist(string apDir, string world)
    {
        var worldInWorlds = File.Exists($"{apDir}/custom_worlds/{world}.apworld");
        var worldInLibWorlds = File.Exists($"{apDir}/lib/worlds/{world}.apworld");
        if (worldInLibWorlds ^ worldInWorlds) return true;
        MainController.ShowError(
            worldInWorlds ? "Duplicate ApWorld in ./custom_worlds and ./lib/worlds" : $"ApWorld [{world}] not found"
        );
        return false;
    }
}