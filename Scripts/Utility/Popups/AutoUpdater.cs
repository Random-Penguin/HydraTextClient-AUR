using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Godot;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility.DataTypes;
using Newtonsoft.Json;
using HttpClient = System.Net.Http.HttpClient;

namespace HydraTextClient.Scripts.Utility.Popups;

public partial class AutoUpdater : WindowSetter
{
    public const string GithubReleasesPath = "https://github.com/SWCreeperKing/HydraTextClient_Rewrite/releases/tag/";
    public HttpClient Client;

    public const string GithubVersionPath
        = "https://github.com/SWCreeperKing/HydraTextClient_Rewrite/blob/master/Version.json";

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
        VersionInfos = JsonConvert.DeserializeObject<List<VersionInfo>>(versionJson)
                                  .Where(info => info != thisVersion && info > thisVersion)
                                  .ToDictionary(i => i.VersionText, i => i);
        
        CurrentVersion = VersionInfos[thisVersion.VersionText];
        MaxVersion = VersionInfos.Values.Aggregate((i1, i2) => i1 > i2 ? i1 : i2);
        return CurrentVersion != MaxVersion;
    }

    public void Update(Button sender)
    {
        sender.Disabled = true;
        try
        {
            // grab zip
            var selfFile = System.Environment.ProcessPath!;
            var zipType = CurrentVersion.FileHashes[ExternalAppController.GetFileSha(selfFile)];
            var zipPath = $"{Path.GetDirectoryName(System.Environment.ProcessPath)!}/{zipType}";

            using var response = Client.GetStreamAsync($"{GithubReleasesPath}{MaxVersion.VersionText}/{zipType}")
                                       .GetAwaiter().GetResult();
            response.CopyTo(File.Create(zipPath));

            // extract
            File.SetAttributes(selfFile, FileAttributes.Hidden);
            File.Move(selfFile, $"{Path.GetDirectoryName(selfFile)}/_OLD_HYDRA_DELETE_ME");

            using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Read);
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
            File.Delete(zipPath);
        }
        catch (Exception e) { GD.PrintErr(e); }
        MainController.QuitHydra();
    }

    protected override void Dispose(bool disposing) => Client?.Dispose();
}