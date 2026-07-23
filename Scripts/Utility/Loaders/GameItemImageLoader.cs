using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Godot;
using Newtonsoft.Json;
using static HydraTextClient.Scripts.Utility.Loaders.Directories;

namespace HydraTextClient.Scripts.Utility.Loaders;

public static class GameItemImageLoader
{
    private static ConcurrentDictionary<string, ItemImageLoader> GameImages = [];
    private static ConcurrentDictionary<string, ConcurrentDictionary<string, string>> GameImageAliases = [];
    static GameItemImageLoader() => Reload();

    public static void Reload()
    {
        if (!Directory.Exists(GameItemImageOverrides)) Directory.CreateDirectory(GameItemImageOverrides);
        foreach (var folder in Directory.GetDirectories(GameItemImageOverrides))
        {
            var gameName = Path.GetFileNameWithoutExtension(folder)!.ToLower();
            GD.Print($"Loading [{gameName}] assets");
            GameImages[gameName] = new ItemImageLoader(folder, gameName);
            var aliases = $"{folder}/aliases.json";
            if (!File.Exists(aliases)) continue;
            var aliasDict = GameImageAliases[gameName] = [];
            foreach (var alias in JsonConvert.DeserializeObject<AliasGroups>(File.ReadAllText(aliases)).Aliases)
            foreach (var item in alias.ItemNames)
                aliasDict.TryAdd(item.ToLower(), alias.AliasName.ToLower());
        }
    }

    public static bool TryGet(string gameName, string itemName, out ImageTexture img)
    {
        img = null;
        if (GameImageAliases.TryGetValue(gameName, out var aliasGroup)
            && aliasGroup.TryGetValue(itemName.ToLower(), out var alias)) itemName = alias;

        return GameImages.TryGetValue(gameName.ToLower().Replace(":", ""), out var imgLoader)
               && imgLoader.TryGet(itemName.ToLower(), out img);
    }
}

public class ItemImageLoader(string dir, string gameName) : ImageLoader
{
    public string GameName = gameName;
    public override string ImageFolder => dir;
    public override bool LoadSubDirectories => false;
    public override string NameModify(string name) => name.ToLower().Replace($"{GameName}_", "");
}

public struct AliasGroups(Alias[] aliases)
{
    public Alias[] Aliases = aliases;
}

public struct Alias(string aliasName, string[] itemNames)
{
    public string AliasName = aliasName.ToLower();
    public string[] ItemNames = itemNames.Select(img => img.ToLower()).ToArray();
}