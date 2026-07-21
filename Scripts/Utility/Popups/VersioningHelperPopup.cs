using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Godot;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility.DataTypes;
using Newtonsoft.Json;

namespace HydraTextClient.Scripts.Utility.Popups;

public partial class VersioningHelperPopup : WindowSetter
{
    public const string BridgeApWorldPath = "D:/___ARCH/____HYDRA_BRIDGE/HydraUTBridge.apworld";
    public const string JsonPath = "E:/coding projects/Godot/hydra-text-client/Version.json";
    public const string BuildPath = "E:/coding projects/Godot/hydra-text-client/Builds";
    public List<VersionInfo> VersionInfos = [];

    public static Dictionary<string, string> FileTypes = new()
    {
        ["HydraTextClient.exe"] = "Windows.zip", ["HydraTextClient.x86_64"] = "Linux.zip",
        ["HydraTextClient.arm64"] = "Linux_arm64.zip",
    };

    public static Dictionary<string, string> FileTypesReverse = FileTypes.ToDictionary(kv => kv.Value, kv => kv.Key);

    [Export] private Label Version;
    [Export] private CodeEdit Changes;

    public override void _Ready()
    {
        Version.Text = MainController.GetVersion();
        if (!File.Exists(JsonPath)) return;
        VersionInfos = JsonConvert.DeserializeObject<List<VersionInfo>>(File.ReadAllText(JsonPath));
    }

    public void ExportJson()
    {
        var files = Directory.GetFiles(BuildPath).Where(file => !(file is "Licenses" || file.EndsWith(".zip")))
                             .ToArray();

        foreach (var file in files)
        {
            var zip = $"{BuildPath}/{FileTypes[Path.GetFileName(file)]}.zip";
            if (File.Exists(zip)) File.Delete(zip);
            using var archive = ZipFile.Open($"{BuildPath}/{FileTypes[Path.GetFileName(file)]}", ZipArchiveMode.Create);
            archive.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
            foreach (var license in Directory.GetFiles($"{BuildPath}/Licenses"))
            {
                archive.CreateEntryFromFile(license, $"Licenses/{Path.GetFileName(license)}", CompressionLevel.Optimal);
            }
            break;
        }

        var newInfo = VersionInfo.CreateFrom(
            MainController.GetVersion(), Changes.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries),
            files.ToDictionary(ExternalAppController.GetFileSha, file => FileTypes[Path.GetFileName(file)])
        );

        VersionInfos.RemoveAll(ver => ver.ExtVersion == newInfo.ExtVersion);
        VersionInfos.Add(newInfo);
        File.WriteAllText(JsonPath, JsonConvert.SerializeObject(VersionInfos));
        DisplayServer.ClipboardSet(string.Join('\n', newInfo.Content));

        Close();
    }

    public void CopyBridgeApWorldHash()
        => DisplayServer.ClipboardSet(ExternalAppController.GetFileSha(BridgeApWorldPath));
}