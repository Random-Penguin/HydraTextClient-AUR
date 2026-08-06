using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HydraTextClient.Scripts.Mapper;


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

public struct LocationGroup(string name, string openIcon = "", string closeIcon = "", params List<string> locations)
{
    public string GroupName = name;
    public string AvailableIcon = openIcon;
    public string CollectedIcon = closeIcon;
    public List<string> Locations = locations;
    // todo: add slot data conditions here
}