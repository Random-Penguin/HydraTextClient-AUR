using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Godot;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.UIHelpers;
using Newtonsoft.Json;
using HttpClient = System.Net.Http.HttpClient;

namespace HydraTextClient.Scripts.Utility.Popups;

public partial class AutoUpdater : WindowSetter
{
    public const string GithubReleasesPath
        = "https://github.com/SWCreeperKing/HydraTextClient_Rewrite/releases/download/";

    public HttpClient Client;

    public const string GithubVersionPath
        = "https://raw.githubusercontent.com/SWCreeperKing/HydraTextClient_Rewrite/refs/heads/master/Version.json";

    [Export] private TabContainer Container;

    public VersionInfo MaxVersion;
    public VersionInfo CurrentVersion;
    public Dictionary<string, VersionInfo> VersionInfos;

    public override void _Ready()
    {
        Title = $"[{MainController.GetVersion()}] -> [{MaxVersion.VersionText}]";

        while (VersionInfos.Count != 0)
        {
            var lowest = VersionInfos.Values.Aggregate((i1, i2) => i1 < i2 ? i1 : i2);
            AddVersion(lowest);
            VersionInfos.Remove(lowest.VersionText);
        }
    }

    private void AddVersion(VersionInfo versionInfo)
    {
        ScrollContainer scroll = new();
        scroll.Name = versionInfo.VersionText;

        Label label = new();
        label.Text = string.Join("\n", versionInfo.Content);

        scroll.AddChild(label);
        Container.AddChild(scroll);
    }

    public bool CanRunUpdater()
    {
        var selfFile = System.Environment.ProcessPath;
        if (Path.GetFileNameWithoutExtension(selfFile)!.ToLower() is "godot") return false;

        Client = new HttpClient();
        using var response = Client.GetAsync(GithubVersionPath).GetAwaiter().GetResult();
        if (response.StatusCode is HttpStatusCode.NotFound) { return false; }
        if (response.StatusCode is not HttpStatusCode.OK)
        {
            MainController.ShowError(
                $"Failed to Check for Updates: code: [{response.StatusCode}] [{(int)response.StatusCode}]"
            );
            return false;
        }

        using var content = response.Content;
        var versionJson = content.ReadAsStringAsync().GetAwaiter().GetResult();
        var thisVersion = VersionInfo.CreateEmpty(MainController.GetVersion());
        var raw = JsonConvert.DeserializeObject<List<VersionInfo>>(versionJson);
        VersionInfos = raw
                      .Where(info => info == thisVersion || info > thisVersion)
                      .ToDictionary(i => i.VersionText, i => i);
        VersionInfos.Remove(thisVersion.VersionText, out CurrentVersion);

        if (!SaveType<bool>.Load(MainController.UpdateToBeta, false) && VersionInfos.Count > 0)
        {
            var mainVersions = VersionInfos.Values.Where(info => info.ExtVersion is "").ToArray();
            switch (mainVersions.Length)
            {
                case 0 when CurrentVersion.ExtVersion is "": VersionInfos.Clear(); break;
                case > 0 when CurrentVersion.ExtVersion is "":
                    VersionInfos = mainVersions.ToDictionary(info => info.VersionText, info => info); break;
                case > 0 when CurrentVersion.ExtVersion is not "":
                    var highestMain = mainVersions.Aggregate((i1, i2) => i1 > i2 ? i1 : i2);
                    VersionInfos = VersionInfos.Where(kv => kv.Value == highestMain || kv.Value < highestMain)
                                               .ToDictionary(kv => kv.Key, kv => kv.Value);
                    break;
            }
        }

        if (VersionInfos.Count == 0)
        {
            GD.Print("No new updates");
            return false;
        }
        
        MaxVersion = VersionInfos.Values.Aggregate((i1, i2) => i1 > i2 ? i1 : i2);
        if (CurrentVersion == MaxVersion) GD.Print("No new updates");
        return CurrentVersion != MaxVersion;
    }

    public void Update(ButtonAnimation sender)
    {
        sender.Disabled = true;
        try
        {
            // grab zip
            var selfFile = System.Environment.ProcessPath!;
            var zipType = CurrentVersion.FileHashes[ExternalAppController.GetFileSha(selfFile)];
            var zipPath = $"{Path.GetDirectoryName(System.Environment.ProcessPath)!}/{zipType}";

            var response = Client.GetByteArrayAsync($"{GithubReleasesPath}{MaxVersion.VersionText}/{zipType}")
                                 .GetAwaiter().GetResult();
            if (File.Exists(zipPath)) File.Delete(zipPath);
            File.WriteAllBytes(zipPath, response);

            // extract
            File.Move(selfFile, $"{Path.GetDirectoryName(selfFile)}/_OLD_HYDRA_DONT_USE_WILL_AUTODELETE");

            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Read))
            {
                var entry = zip.GetEntry(VersioningHelperPopup.FileTypesReverse[zipType]);
                if (entry is null)
                {
                    MainController.ShowError(
                        $"Item [{zipType}] in filetypereverse gave [{VersioningHelperPopup.FileTypesReverse[zipType]}], not present in zip"
                    );
                    Close();
                    return;
                }

                entry!.ExtractToFile(selfFile);
            }

            File.Delete(zipPath);
            MainController.QuitHydra();
        }
        catch (Exception e) { GD.PrintErr(e); }
    }

    protected override void Dispose(bool disposing) => Client?.Dispose();
}