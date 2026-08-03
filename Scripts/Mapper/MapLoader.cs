using System;
using System.Collections.Generic;
using System.IO;
using CreepyUtil.Archipelago.ApClient;
using Godot;
using HydraTextClient.Scripts.Clients.CircleTracker;
using Newtonsoft.Json;

namespace HydraTextClient.Scripts.Mapper;

public partial class MapLoader : Control
{
    [Export] public PackedScene MapLocation;
    [Export] public PackedScene MapContainer;
    public List<Maps> MapsList = [];
    public TabStructure Structure;
    public Dictionary<string, TabContainer> MapTabs = [];
    public ApClient Client;
    public TrackerPage Page;

    public void Setup(string path, string trackerName)
    {
        MapsList = JsonConvert.DeserializeObject<List<Maps>>(File.ReadAllText($"{path}/atlas.json"));
        Structure = JsonConvert.DeserializeObject<TabStructure>(File.ReadAllText($"{path}/tabs.json"));
        // CircleTracker.Singleton.;

        Queue<TabStructure> structures = [];
        structures.Enqueue(Structure);

        while (structures.Count != 0)
        {
            var tab = structures.Dequeue();
            foreach (var child in tab.SubTabs) structures.Enqueue(child with { Parent = tab.Name });
            if (MapTabs.ContainsKey(tab.Name)) continue;

            var container = MapTabs[tab.Name] = new TabContainer();

            if (tab.Name is "")
            {
                AddChild(container);
                continue;
            }

            container.Name = tab.Name;
            MapTabs[tab.Parent].AddChild(container);
        }

        foreach (var map in MapsList)
        {
            var container = MapTabs.GetValueOrDefault(map.Tab, MapTabs[""]);

            var mapContainer = MapContainer.Instantiate<MapNavigator>();
            mapContainer.Name = map.MapName;
            mapContainer.SetImage(
                ImageTexture.CreateFromImage(Image.LoadFromFile($"{path}/maps/{MapsList[0].ImageName}"))
            );

            foreach (var loc in map.Nodes)
            {
                var node = MapLocation.Instantiate<MapLocation>();
                node.Locations = loc.Locations;
                node.Name = loc.Name;
                node.SetImage(path, loc.Icon, loc.Size, this);

                mapContainer.Container.MapImage.AddChild(node);
                node.Position = loc.Position;
            }

            container.AddChild(mapContainer);
        }
    }

    public void RemoveNode(MapLocation node)
    {
        
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
    params List<LocationCheck> locationChecks)
{
    public string Icon = icon;
    public string Name = name;
    public List<LocationCheck> Locations = locationChecks;
    [JsonIgnore] public Vector2 Position = new(x, y);
    [JsonIgnore] public Vector2 Size = new(w, h);

    public float X { get => Position.X; set => Position = Position with { X = value }; }
    public float Y { get => Position.Y; set => Position = Position with { Y = value }; }
    public float W { get => Size.X; set => Size = Size with { X = value }; }
    public float H { get => Size.Y; set => Size = Size with { Y = value }; }
}

public struct LocationCheck(string loc, string icon = "") : IEquatable<LocationCheck>
{
    public string Icon = icon;
    public string Location = loc;

    public bool Equals(LocationCheck other) => Icon == other.Icon && Location == other.Location;
    public override bool Equals(object obj) => obj is LocationCheck other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Icon, Location);
}