using System.Collections.Generic;
using System.IO;
using System.Linq;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.Popups;
using Newtonsoft.Json;

namespace HydraTextClient.Scripts.Mapper;

public partial class PoptrackerImporter : WindowSetter
{
    private string PackPath;
    private PoptrackerManifest Manifest;
    private PoptrackerMap[] MapsFile;
    private Dictionary<string, string> MapToFileConversion = [];
    private Dictionary<int, TabStructure> LayoutCandidates = [];
    private Dictionary<int, Dictionary<string, Maps>> LayoutCandidateMaps = [];
    
    public void ReadPack(string manifestFile)
    {
        PackPath = Path.GetDirectoryName(manifestFile);
        Manifest = JsonConvert.DeserializeObject<PoptrackerManifest>(File.ReadAllText(manifestFile));

        if (Directory.GetDirectories(Directories.MapPacks).Select(s => s.ToLower()).Contains(Manifest.GameName.Replace(":", "").ToLower()))
        {
            MainController.ShowError($"Pack for [{Manifest.GameName}] already exists");
            CallDeferred("Close");
            return;
        }
        
        MapsFile = JsonConvert.DeserializeObject<PoptrackerMap[]>(File.ReadAllText($"{PackPath}/maps.json"));

        foreach (var map in MapsFile) MapToFileConversion[map.Name] = Path.GetFileName(map.Image);
        foreach (var layoutPath in Directory.GetFiles($"{PackPath}/layouts"))
        {
            if (!layoutPath.ToLower().EndsWith(".json")) continue;
            var parentLayout = JsonConvert.DeserializeObject<PoptrackerLayout>(File.ReadAllText(layoutPath));

            Queue<PoptrackerLayout> searchQueue = [];
            searchQueue.Enqueue(parentLayout);

            while (searchQueue.Count != 0)
            {
                var layout = searchQueue.Dequeue();

                if (IsLayoutAMapTab(layout))
                {
                    var map = GenerateLayout(layout, MapToFileConversion, out var data);
                    var id = map.GetHashCode();
                    LayoutCandidates[id] = map;
                    LayoutCandidateMaps[id] = data;

                    searchQueue.Clear();
                    break;
                }

                foreach (var child in layout.Content) searchQueue.Enqueue(child);
            }
        }
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
            if (layout.Content.Length < 1) return false;

            foreach (var child in layout.Content) searchQueue.Enqueue(child);
        }
        return false;
    }

    private TabStructure GenerateLayout(PoptrackerLayout parentLayout, Dictionary<string, string> mapConversion,
        out Dictionary<string, Maps> mapData)
    {
        mapData = [];
        Dictionary<string, TabStructure> tabs = new() { [""] = new TabStructure("") };
        Queue<(PoptrackerLayout layout, string parent)> convertQueue = [];

        foreach (var child in parentLayout.Content) convertQueue.Enqueue((child, ""));

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

                case "":
                    if (layout.Content.Length < 1) continue;
                    var map = layout.Content[0];
                    if (map.Type is not "map" || map.Maps.Length < 1) continue;
                    if (!mapConversion.TryGetValue(map.Maps[0], out var mapImg)) continue;
                    mapData[layout.Title] = new Maps(layout.Title, mapImg, parent);
                    break;
            }
        }

        return tabs[""];
    }
}