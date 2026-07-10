using System;
using System.Collections.Generic;
using Godot;

namespace HydraTextClient.Scripts.Settings;

public partial class SettingsCreator : TabContainer
{
    private static HashSet<string> TabsNames = [];
    private static PriorityQueue<string, int> TabCreationPriority = new();
    private static Dictionary<string, List<Action<SettingsContainer>>> TabCreateCallback = [];
    private Dictionary<string, SettingsContainer> Tabs = [];

    public override void _Process(double delta)
    {
        while (TabCreationPriority.Count is not 0)
        {
            var tab = TabCreationPriority.Dequeue();
            var tabContainer = this[tab];
            if (!TabCreateCallback.TryGetValue(tab, out var actions)) continue;
            if (actions.Count == 0) continue;
            foreach (var action in actions) action?.Invoke(tabContainer);
        }
    }

    public SettingsContainer this[string tabName]
    {
        get
        {
            if (Tabs.TryGetValue(tabName, out var tab)) return tab;
            tab = Tabs[tabName] = new SettingsContainer();
            tab.SetAnchorsPreset(LayoutPreset.FullRect);
            tab.SetName(tabName);
            AddChild(tab);
            return tab;
        }
    }

    public static void Tab(string tabName, Action<SettingsContainer>? callback = null, int priority = 1000000)
    {
        if (TabsNames.Add(tabName)) TabCreationPriority.Enqueue(tabName, priority);
        if (callback is null) return;
        if (TabCreateCallback.TryGetValue(tabName, out var value)) value.Add(callback);
        else TabCreateCallback[tabName] = [callback];
    }
}