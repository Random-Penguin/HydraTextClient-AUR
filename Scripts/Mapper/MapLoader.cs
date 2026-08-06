using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Archipelago.MultiClient.Net.Models;
using CreepyUtil.Archipelago.ApClient;
using Godot;
using HydraTextClient.Scripts.Clients.CircleTracker;
using HydraTextClient.Scripts.Utility.UIHelpers;
using Newtonsoft.Json;

namespace HydraTextClient.Scripts.Mapper;

public partial class MapLoader : Control
{
    [Export] public Control Container;
    [Export] public ItemList List;
    [Export] public PackedScene MapLocation;
    [Export] public PackedScene MapContainer;
    public MapItemImageLoader ItemImageLoader;
    public List<Maps> MapsList = [];
    public TabStructure Structure;
    public Dictionary<string, TabContainer> MapTabs = [];
    public Dictionary<int, MapLocation> MapLocationMap = [];
    public Dictionary<int, MapNavigator> MapNavMap = [];
    public ApClient Client;
    public TrackerPage Page;
    public MapTracker Parent;
    private string TrackerName;
    private HashSet<int> SelectedLocation = [];
    private HashSet<int> HoveredLocation = [];
    private bool UpdateItemList;
    private EmptyRichLabelInteractor LocationPopupList;

    public void Setup(string path, string trackerName, MapTracker parent)
    {
        Client.HintsTrackedEvent += UpdateNodes;

        TrackerName = trackerName;
        Parent = parent;
        MapsList = JsonConvert.DeserializeObject<List<Maps>>(File.ReadAllText($"{path}/atlas.json"));
        Structure = JsonConvert.DeserializeObject<TabStructure>(File.ReadAllText($"{path}/tabs.json"));
        if (!CircleTracker.Singleton.Pages.TryGetValue(trackerName, out Page))
        {
            parent.UnloadMap(trackerName);
            return;
        }
        ItemImageLoader = new(path);

        Page.OnLogicUpdated += UpdateNodes;

        Queue<TabStructure> structures = [];
        structures.Enqueue(Structure);

        while (structures.Count != 0)
        {
            var tab = structures.Dequeue();
            foreach (var child in tab.SubTabs) structures.Enqueue(child with { Parent = tab.Name });
            if (MapTabs.ContainsKey(tab.Name)) continue;

            var container = MapTabs[tab.Name] = new TabContainer();
            container.SizeFlagsVertical = SizeFlags.ExpandFill;

            if (tab.Name is "")
            {
                Container.AddChild(container);
                continue;
            }

            container.Name = tab.Name;
            MapTabs[tab.Parent].AddChild(container);
        }

        var nodeId = -1;
        var mapId = -1;
        foreach (var map in MapsList)
        {
            mapId++;
            var container = MapTabs.GetValueOrDefault(map.Tab, MapTabs[""]);

            var mapContainer = MapNavMap[mapId] = MapContainer.Instantiate<MapNavigator>();
            mapContainer.Name = map.MapName;
            var image = ImageTexture.CreateFromImage(Image.LoadFromFile($"{path}/maps/{MapsList[0].ImageName}"));
            mapContainer.SetImage(image);
            var imageSize = image.GetSize();

            foreach (var loc in map.Nodes)
            {
                nodeId++;
                var id = nodeId;
                var node = MapLocation.Instantiate<MapLocation>();
                node.Locations = loc.Locations;
                node.Name = loc.Name;
                var nodeSize = new Vector2(Math.Abs(loc.W), Math.Abs(loc.H));
                node.SetImage(mapId, nodeId, path, loc.Icon, nodeSize, this);

                if (loc.Icon is not "" && ItemImageLoader.TryGet(loc.Icon, out var img))
                {
                    node.Texture = img;
                    node.HasCustomImage = true;
                }
                else if (loc.Icon is not "") GD.PrintErr($"Location Icon not found for: [{loc.Icon}]");

                node.OnEntered += () =>
                {
                    HoveredLocation.Add(id);
                    UpdateItemList = true;
                };
                node.OnExited += () =>
                {
                    HoveredLocation.Remove(id);
                    UpdateItemList = true;
                };
                node.OnSelected += () =>
                {
                    SelectedLocation.Add(id);
                    UpdateItemList = true;
                };
                node.OnUnSelected += () =>
                {
                    SelectedLocation.Remove(id);
                    UpdateItemList = true;
                };

                mapContainer.Container.MapImage.AddChild(node);
                MapLocationMap[nodeId] = node;

                var nodePos = new Vector2(
                    Math.Clamp(loc.X, nodeSize.X / 2f, imageSize.X - nodeSize.X / 2f),
                    Math.Clamp(loc.Y, nodeSize.Y / 2f, imageSize.Y - nodeSize.Y / 2f)
                );
                node.Position = nodePos;
            }

            container.AddChild(mapContainer);
        }
    }

    public override void _Process(double delta)
    {
        if (!UpdateItemList) return;
        List.Visible = false;
        UpdateItemList = false;

        if (SelectedLocation.Count != 0)
        {
            SetItemList(SelectedLocation.First());
            return;
        }

        if (HoveredLocation.Count == 0) return;
        SetItemList(HoveredLocation.First());
    }

    public void SetItemList(int locationId)
    {
        List.Visible = true;
        List.Clear();

        var node = MapLocationMap[locationId];
        foreach (var loc in node.Locations)
        {
            if (!Client.MissingLocations.Contains(loc)) return;
            var i = List.AddItem(loc);
            if (!node.HasCustomImage) continue;
            List.SetItemIcon(i, node.Texture);
        }
    }

    public void ResetSelectedNodes()
    {
        foreach (var id in SelectedLocation) MapLocationMap[id].EmitUnSelect();
        SelectedLocation.Clear();
        UpdateItemList = true;
    }

    public void UpdateNodes(Hint[] hints) => UpdateNodes();

    public void UpdateNodes()
    {
        foreach (var node in MapLocationMap.Values) node.QueueUpdate = true;
    }

    public void RemoveNode(int mapId, int nodeId)
    {
        var node = MapLocationMap[nodeId];
        MapLocationMap.Remove(nodeId);
        MapNavMap[mapId].Container.MapImage.RemoveChild(node);
        node.QueueFree();
    }

    public void StopAndClose() => Parent.CallDeferred("UnloadMap", TrackerName);

    public void ResetZoom()
    {
        var container = MapTabs[""];
        while (true)
        {
            if (container.GetChildren().Count == 0) return;
            switch (container.GetChild(container.CurrentTab))
            {
                case TabContainer newContainer: container = newContainer; break;
                case MapNavigator nav:
                    nav.Container.ResetZoom();
                    return;
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        Client?.HintsTrackedEvent -= UpdateNodes;
        Page?.OnLogicUpdated -= UpdateNodes;
    }
}

public struct TabStructure(string name = "", params List<TabStructure> subTabs)
{
    public string Name = name;
    public List<TabStructure> SubTabs = subTabs;
    [JsonIgnore] public string Parent;
}

public struct Maps(string mapName, string imageName, string tab = "", params List<MapNode> nodes)
{
    public string MapName = mapName;
    public string ImageName = imageName;
    public string Tab = tab;
    public List<MapNode> Nodes = nodes;
}

public struct MapNode(string name, float x, float y, float w = 16, float h = 16, string icon = "",
    params List<string> locationChecks)
{
    public string Icon = icon;
    public string Name = name;
    public List<string> Locations = locationChecks;
    public float X = x;
    public float Y = y;
    public float W = w;
    public float H = h;
}