using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using static HydraTextClient.Scripts.Utility.Loaders.Directories;

namespace HydraTextClient.Scripts.Utility.Loaders;

public static class GamePortraitLoader
{
    public static event Action? OnReloadImages;
    private static Dictionary<string, ImageTexture> GamePortraitImages = [];
    private static HashSet<string> BaseList = [];
    public static string[] GameList = [];

    static GamePortraitLoader() => ReloadImages();

    public static void ReloadImages()
    {
        GD.Print("Loading Portraits");
        if (!Directory.Exists(GamePortraits)) Directory.CreateDirectory(GamePortraits);
        LoadDirectory(GamePortraits);
        GameList = BaseList.Order().ToArray();
        OnReloadImages?.Invoke();
    }

    private static void LoadDirectory(string dir)
    {
        foreach (var file in Directory.GetFiles(dir))
        {
            var gameName = string.Join(".", file.Replace("\\", "/").Split("/")[^1].Split('.')[..^1]);
            if (GamePortraitImages.ContainsKey(gameName)) continue;
            GamePortraitImages[CleanName(gameName)] = ImageTexture.CreateFromImage(Image.LoadFromFile(file));
            BaseList.Add(gameName);
            GD.Print($"Loaded image [{file.Replace(dir, ".")}] for game [{gameName}]");
        }

        foreach (var subDir in Directory.GetDirectories(dir)) LoadDirectory(subDir);
    }

    public static bool TryGet(string gameName, out ImageTexture img)
        => GamePortraitImages.TryGetValue(CleanName(gameName), out img);

    public static Texture2D GetOrDef(string gameName, Texture2D def)
    {
        if (!GamePortraitImages.ContainsKey(CleanName(gameName))) return def;
        return GamePortraitImages[CleanName(gameName)];
    }

    public static ImageTexture GetImage(string name) => GamePortraitImages[name.ToLower()];
    public static string GameAt(int i) => GameList[i];
    private static string CleanName(string name) => name.ToLower().Replace(":", "");
}