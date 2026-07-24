using System.Collections.Generic;
using Godot;
using HydraTextClient.Scripts.Controllers;

namespace HydraTextClient.Scripts.Utilities;

public partial class Utilities : TabContainer
{
    [Export] private PackedScene SlotUtilScene;
    private Dictionary<string, SlotUtility> UtilityPages = [];

    public override void _Ready()
    {
        ConnectionController.OnClientConnection += (name, _, _) => CallDeferred("OpenPage", name);
        ConnectionController.OnClientRemoved += (name, _, _) => CallDeferred("ClosePage", name);
    }

    public void OpenPage(string name)
    {
        var client = ConnectionController.GetClient(name);
        var page = UtilityPages[name] = SlotUtilScene.Instantiate<SlotUtility>();
        page.SetupPlayer(client);
        page.Name = name;
        AddChild(page);
    }

    public void ClosePage(string name)
    {
        var page = UtilityPages[name];
        UtilityPages.Remove(name);
        RemoveChild(page);
        page.QueueFree();
    }
}