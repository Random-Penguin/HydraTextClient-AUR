using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.Popups;
using Newtonsoft.Json;

namespace HydraTextClient.Scripts.Mapper;

public partial class PoptrackerImporter : WindowSetter
{
    [Export] public OptionButton LayoutOptions;
    [Export] public VBoxContainer LocationImports;

    private string PackPath;
    private PoptrackerManifest Manifest;
    private PoptrackerMap[] MapsFile;
    private Dictionary<string, string> MapToFileConversion = [];
    private Dictionary<string, string> MapIdNameToMapName = [];
    private Dictionary<int, TabStructure> LayoutCandidates = [];
    private Dictionary<int, Dictionary<string, Maps>> LayoutCandidateMaps = [];
    private Dictionary<int, string> LayoutNames = [];
    private Dictionary<int, int> OptionItemMap = [];
    private Dictionary<string, CheckBox> LayoutSelections = [];
    private string[] LocationJsons = [];

    public void CallReadPack(string manifest) => CallDeferred("ReadPack", manifest);

    private void ReadPack(string manifestFile)
    {
        PackPath = Path.GetDirectoryName(manifestFile);
        Manifest = JsonConvert.DeserializeObject<PoptrackerManifest>(File.ReadAllText(manifestFile));
        LocationJsons = Directory.GetFiles($"{PackPath}/locations", "*.json", SearchOption.AllDirectories);

        OptionItemMap.Clear();
        LayoutOptions.Clear();
        MapIdNameToMapName.Clear();
        foreach (var (_, selection) in LayoutSelections)
        {
            LocationImports.RemoveChild(selection);
            selection.QueueFree();
        }
        LayoutSelections.Clear();

        if (Directory.GetDirectories(Directories.MapPacks).Select(s => s.ToLower())
                     .Contains(Manifest.GameName.Replace(":", "").ToLower()))
        {
            MainController.ShowError($"Pack for [{Manifest.GameName}] already exists");
            CallDeferred("Close");
            return;
        }

        MapsFile = JsonConvert.DeserializeObject<PoptrackerMap[]>(File.ReadAllText($"{PackPath}/maps/maps.json"));

        var optionItem = 0;
        foreach (var map in MapsFile) MapToFileConversion[map.Name] = Path.GetFileName(map.Image);
        foreach (var layoutPath in Directory.GetFiles($"{PackPath}/layouts"))
        {
            if (!layoutPath.ToLower().EndsWith(".json")) continue;

            var parentLayout = JsonConvert.DeserializeObject<PoptrackerLayout>(File.ReadAllText(layoutPath));
            Queue<PoptrackerLayout> searchQueue = [];
            if (parentLayout.DefaultLayout is not null) searchQueue.Enqueue(parentLayout.DefaultLayout);
            if (parentLayout.HorizontalLayout is not null) searchQueue.Enqueue(parentLayout.HorizontalLayout);
            searchQueue.Enqueue(parentLayout);

            while (searchQueue.Count != 0)
            {
                var layout = searchQueue.Dequeue();

                if (IsLayoutAMapTab(layout))
                {
                    var map = GenerateLayout(layout, out var data);
                    var id = map.GetHashCode();
                    LayoutCandidates[id] = map;
                    LayoutCandidateMaps[id] = data;
                    LayoutNames[id] = Path.GetFileNameWithoutExtension(layoutPath);
                    LayoutOptions.AddItem(LayoutNames[id], optionItem);
                    OptionItemMap[optionItem] = id;
                    optionItem++;

                    searchQueue.Clear();
                    break;
                }

                foreach (var child in layout.Content) searchQueue.Enqueue(child);
            }
        }

        foreach (var json in LocationJsons)
        {
            CheckBox box = new();
            box.Name = json;
            box.Text = Path.GetFileName(json);
            box.ButtonPressed = true;
            LocationImports.AddChild(box);
            LayoutSelections[json] = box;
        }
    }

    public void FinishConversionAndClose()
    {
        GD.Print(JsonConvert.SerializeObject(MapIdNameToMapName));

        var path = $"{Directories.MapPacks}/{Manifest.GameName}";
        var chosenLayout = OptionItemMap[LayoutOptions.Selected];
        Directory.CreateDirectory(path);
        Directory.CreateDirectory($"{path}/images");
        Directory.CreateDirectory($"{path}/maps");
        var maps = LayoutCandidateMaps[chosenLayout]; // atlas.json
        Dictionary<string, Dictionary<int, MapNode>> mapNodes = [];

        foreach (var map in MapsFile)
        {
            if (File.Exists($"{path}/{map.Image}")) continue;
            File.Copy($"{PackPath}/{map.Image}", $"{path}/{map.Image}");
        }

        foreach (var (id, checkBox) in LayoutSelections)
        {
            if (!checkBox.ButtonPressed) continue;
            var file = File.ReadAllText(id);

            Queue<PoptrackerLocation> locationsToCheck = [];
            try
            {
                var locations = JsonConvert.DeserializeObject<PoptrackerLocation[]>(file);
                foreach (var loc in locations) locationsToCheck.Enqueue(loc);
            }
            catch (Exception e)
            {
                try
                {
                    var locations = JsonConvert.DeserializeObject<PoptrackerLocation>(file);
                    locationsToCheck.Enqueue(locations);
                }
                catch (Exception ee) { GD.PrintErr($"Error converting [{id}]", e, "original: ", ee); }
            }

            while (locationsToCheck.Count != 0)
            {
                var loc = locationsToCheck.Dequeue();

                if (loc.MapLocations is not null)
                {
                    foreach (var mapLoc in loc.MapLocations)
                    {
                        var size = mapLoc.Size;
                        if (size is 0) size = 12;
                        var x = mapLoc.X - size/2;
                        var y = mapLoc.Y - size/2;

                        var mapName = MapIdNameToMapName[mapLoc.MapName];
                        if (!mapNodes.TryGetValue(mapName, out var possibleNodes))
                            mapNodes[mapName] = possibleNodes = [];
                        var posHash = HashCode.Combine(mapLoc.X, mapLoc.Y);
                        if (!possibleNodes.TryGetValue(posHash, out var node))
                        {
                            possibleNodes[posHash] = node = new MapNode(loc.Name, x, y, size, size);

                            if (!maps.ContainsKey(mapName))
                            {
                                GD.PrintErr($"Map [{mapName}] (pos: [{mapLoc.X},{mapLoc.Y}]) does not exist in the layout for [{id}] at [{loc.Name}]");
                                continue;
                            }
                            maps[mapName].Nodes.Add(node);
                        }

                        node.Locations.Add(loc.Name);
                    }
                }

                if (loc.Locations is null) continue;
                foreach (var newLoc in loc.Locations) locationsToCheck.Enqueue(newLoc);
            }
        }

        File.WriteAllText($"{path}/authors.txt", Manifest.Author);
        File.WriteAllText($"{path}/locationgroups.json", "[]");
        File.WriteAllText($"{path}/atlas.json", JsonConvert.SerializeObject(maps.Values.ToArray()));
        File.WriteAllText($"{path}/tabs.json", JsonConvert.SerializeObject(LayoutCandidates[chosenLayout]));
        Close();
    }

    private bool IsLayoutAMapTab(PoptrackerLayout parentLayout)
    {
        Queue<PoptrackerLayout> searchQueue = [];
        searchQueue.Enqueue(parentLayout);

        while (searchQueue.Count != 0)
        {
            var layout = searchQueue.Dequeue();

            if (layout.Type is "map") return true;
            if (layout.Type is not ("tabbed" or "")) return false;
            if (layout.Maps.Length > 0) return true;
            if (layout.Content.Length < 1 && layout.Tabs.Length < 1) return false;

            foreach (var child in layout.Content) searchQueue.Enqueue(child);
            foreach (var child in layout.Tabs) searchQueue.Enqueue(child);
        }
        return false;
    }

    private TabStructure GenerateLayout(PoptrackerLayout parentLayout, out Dictionary<string, Maps> mapData)
    {
        mapData = [];
        Dictionary<string, TabStructure> tabs = new() { [""] = new TabStructure("") };
        Queue<(PoptrackerLayout layout, string parent)> convertQueue = [];

        if (parentLayout.Content is not null)
            foreach (var child in parentLayout.Content)
                convertQueue.Enqueue((child, ""));
        if (parentLayout.Tabs is not null)
            foreach (var child in parentLayout.Tabs)
                convertQueue.Enqueue((child, ""));

        while (convertQueue.Count > 0)
        {
            var (layout, parent) = convertQueue.Dequeue();
            switch (layout.Type)
            {
                case "tabbed":
                    var parentTab = tabs[parent];
                    if (layout.Title is "") continue;
                    tabs[layout.Title] = new TabStructure(layout.Title);
                    parentTab.SubTabs.Add(tabs[layout.Title]);
                    foreach (var content in layout.Content) convertQueue.Enqueue((content, layout.Title));
                    break;

                case "" or null:
                    if (layout.Content.Length < 1) continue;
                    var map = layout.Content[0];
                    if (map.Type is not "map" || map.Maps.Length < 1) continue;
                    if (!MapToFileConversion.TryGetValue(map.Maps[0], out var mapImg)) continue;
                    mapData[layout.Title] = new Maps(layout.Title, mapImg, parent);
                    MapIdNameToMapName[map.Maps[0]] = layout.Title;
                    break;
            }
        }

        return tabs[""];
    }
}