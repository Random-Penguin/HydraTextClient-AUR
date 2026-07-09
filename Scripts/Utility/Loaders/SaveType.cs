using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HydraTextClient.Scripts.Controllers;
using Newtonsoft.Json;

namespace HydraTextClient.Scripts.Utility.Loaders;

public static class SaveType<T>
{
    private static string SaveDir = $"{Directories.MainDirectory}/Types";
    private static string SaveFile = $"{SaveDir}/type_{typeof(T).Name}.json";
    private static Dictionary<string, T> SaveItems = [];
    public static event Action<string, T>? OnSaveEvent;
    public static event Action<string, T>? OnDeleteEvent;

    static SaveType()
    {
        if (!Directory.Exists(SaveDir)) Directory.CreateDirectory(SaveDir);
        LoadFromFile();
        if (SaveItems is null) SaveItems = [];
        MainController.OnLateSave += SaveToFile;
    }

    public static void Save(string id, T value, bool broadcast)
    {
        SaveItems[id] = value;
        if (broadcast) OnSaveEvent?.Invoke(id, value);
    }
    
    public static void Delete(string key)
    {
        if (!SaveItems.Remove(key, out var item)) return;
        OnDeleteEvent?.Invoke(key, item);
    }

    public static T Load(string id, T def, bool saveDefault = true)
    {
        if (TryGet(id, out var val)) return val;
        if (saveDefault) return SaveItems[id] = def;
        return def;
    }

    public static string[] GetKeys() => SaveItems.Keys.ToArray();
    public static bool ContainsKey(string id) => SaveItems.ContainsKey(id);
    public static bool TryGet(string id, out T val) => SaveItems.TryGetValue(id, out val);
    
    private static void SaveToFile()
        => File.WriteAllText(SaveFile, JsonConvert.SerializeObject(SaveItems, Formatting.Indented));

    private static void LoadFromFile()
    {
        if (!File.Exists(SaveFile)) return;
        SaveItems = JsonConvert.DeserializeObject<Dictionary<string, T>>(File.ReadAllText(SaveFile));
    }
}