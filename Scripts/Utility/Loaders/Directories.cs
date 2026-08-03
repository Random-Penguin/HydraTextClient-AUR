using System;
using System.IO;

namespace HydraTextClient.Scripts.Utility.Loaders;

public static class Directories
{
    public static bool IsPortable => Path.GetFileNameWithoutExtension(Environment.ProcessPath!)!.EndsWith("_Portable");
    
    public static string MainDirectory
    {
        get
        {
            var mainPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (IsPortable) mainPath = Path.GetDirectoryName(Environment.ProcessPath!);
            return $"{mainPath}/HydraTextClient";
        }
    }

    public static string GamePortraits = $"{MainDirectory}/Game Portraits";
    public static string Emotes = $"{MainDirectory}/Emotes";
    public static string GameItemImageOverrides = $"{MainDirectory}/Game Item Images";
    public static string LegacyData = $"{MainDirectory}/data.json";
    public static string MapPacks = $"{MainDirectory}/Map Packs";
}