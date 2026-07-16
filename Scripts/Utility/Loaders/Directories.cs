using System;

namespace HydraTextClient.Scripts.Utility.Loaders;

public static class Directories
{
    public static string MainDirectory
        = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/HydraTextClient";

    public static string GamePortraits = $"{MainDirectory}/Game Portraits";
    public static string Emotes = $"{MainDirectory}/Emotes";
    public static string LegacyData = $"{MainDirectory}/data.json";
}