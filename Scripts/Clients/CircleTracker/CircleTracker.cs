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
    private const string HydraUTBridgeFileHash = "18E9D2425D9158EB04BF369ADE8A988BEFA46836FE479CD1F7162777F49E4F9B";
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
        button.Pressed += () => button.Disabled = OpenTracker(name);
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

    public bool OpenTracker(string name)
    {
        var apDir = SaveType<string>.Load(GlobalThemeSettings.ApDir, "");
        if (apDir is "" || !Directory.Exists(apDir))
        {
            MainController.ShowError(
                apDir is "" ? "Archipelago Directory not set, set it in the Settings/Main Settings"
                    : "Invalid Archipelago Directory"
            );
            return false;
        }

        if (!DoesApWorldExist(apDir, "HydraUTBridge", out var bridgeLoc)) return false;

        if (ExternalAppController.GetFileSha(bridgeLoc) != HydraUTBridgeFileHash)
        {
            MainController.ShowError("HydraUTBridge.apworld version is not compatible with the current Hydra version");
            return false;
        }

        if (!DoesApWorldExist(apDir, "tracker", out _)) return false;

        var page = TrackerScene.Instantiate<TrackerPage>();
        HydraBridgeEntry entry;
        try { entry = new HydraBridgeEntry(apDir, Clients[name], page, true); }
        catch (Exception e)
        {
            MainController.ShowError($"Error with [{apDir}]", e);
            page.QueueFree();
            return false;
        }

        if (!entry.FileExists())
        {
            try { entry = new HydraBridgeEntry(apDir, Clients[name], page, false); }
            catch (Exception e)
            {
                MainController.ShowError($"Error with [{apDir}]", e);
                page.QueueFree();
                return false;
            }
        }

        if (!entry.FileExists())
        {
            MainController.ShowError("The selected folder is not the Archipelago Folder (folder invalid)");
            page.QueueFree();
            return false;
        }

        page.OnStopCalled += () =>
        {
            if (Pages.Remove(name, out var node)) PageContainer.CallDeferred("remove_child", node);
            if (Buttons.TryGetValue(name, out var button)) button.Disabled = false;
        };
        Pages.Add(name, page);
        PageContainer.CallDeferred("add_child", page);
        page.Setup(name, Clients[name], entry);
        return true;
    }

    public bool DoesApWorldExist(string apDir, string world, out string path)
    {
        var custom = $"{apDir}/custom_worlds/{world}.apworld";
        var lib = $"{apDir}/lib/worlds/{world}.apworld";
        var worldInWorlds = File.Exists(custom);
        var worldInLibWorlds = File.Exists(lib);
        if (worldInLibWorlds ^ worldInWorlds)
        {
            path = worldInLibWorlds ? lib : custom;
            return true;
        }
        MainController.ShowError(
            worldInWorlds ? "Duplicate ApWorld in ./custom_worlds and ./lib/worlds" : $"ApWorld [{world}] not found"
        );
        path = "";
        return false;
    }
}