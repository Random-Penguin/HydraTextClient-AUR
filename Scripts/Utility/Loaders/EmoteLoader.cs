using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using HydraTextClient.Scripts.Controllers;

namespace HydraTextClient.Scripts.Utility.Loaders;

public static class EmoteLoader
{
    public static event Action? OnReloadImages;
    private static Dictionary<string, ImageTexture> Emotes = [];

    static EmoteLoader() => ReloadImages();

    public static void ReloadImages()
    {
        GD.Print("Loading Emotes");
        if (!Directory.Exists(Directories.Emotes)) Directory.CreateDirectory(Directories.Emotes);
        LoadDirectory(Directories.Emotes);
        OnReloadImages?.Invoke();
    }

    private static void LoadDirectory(string dir)
    {
        foreach (var file in Directory.GetFiles(dir))
        {
            try
            {
                var emoteName = string.Join(".", file.Replace("\\", "/").Split("/")[^1].Split('.')[..^1]);
                if (Emotes.ContainsKey(emoteName)) continue;
                Emotes[emoteName.ToLower()] = ImageTexture.CreateFromImage(Image.LoadFromFile(file));
                GD.Print($"Loaded image [{file.Replace(dir, ".")}] for game [{emoteName}]");
            }
            catch (Exception e) { MainController.ShowError($"Error Loading Emote [{file}]", e); }
        }

        foreach (var subDir in Directory.GetDirectories(dir)) LoadDirectory(subDir);
    }

    public static bool TryGet(string emoteName, out ImageTexture img)
        => Emotes.TryGetValue(emoteName.ToLower(), out img);

    public static Texture2D GetOrDef(string emoteName, Texture2D def)
    {
        if (!Emotes.ContainsKey(emoteName.ToLower())) return def;
        return Emotes[emoteName.ToLower()];
    }

    public static Dictionary<string, ImageTexture> GetImages() => Emotes;
}