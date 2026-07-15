using System;
using System.Linq;
using Godot;
using HydraTextClient.Scripts.Clients.TextClient;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;
using static HydraTextClient.Scripts.Utility.ColorIdConstants;

namespace HydraTextClient.Scripts.Settings;

public static class GlobalThemeSettings
{
    public const string GlobalFontSize = "Theme/Global/FontSize";
    public const string ApDir = "Main/ArchipelagoFolder";

    public static void Init()
    {
        SaveType<double>.OnSaveEvent += (s, d) =>
        {
            if (s is GlobalFontSize) LoadGlobalFont(d);
        };

        CreateSettings();

        SettingsCreator.Tab(
            "Theme",
            tab => tab
                  .AddSetting(
                       SettingType.ButtonAction, "Force (Safety) Save", columnIndex: 2,
                       extraConfig: b => ((Button)b[0]).Pressed += MainController.Save
                   )
                  .AddSetting(
                       SettingType.ButtonAction, "Open Save Directory", columnIndex: 2,
                       extraConfig: b => ((Button)b[0]).Pressed += () => OS.ShellOpen(Directories.MainDirectory)
                   ).AddSeparator(columnIndex: 2)
                  .AddSetting(
                       SettingType.ButtonAction, "Export Colors to Clipboard", columnIndex: 2,
                       extraConfig: b => ((Button)b[0]).Pressed += () => DisplayServer.ClipboardSet(
                           string.Join('|', ConstantToId.Select(kv => $"{kv.Value}={kv.Key.Color().ToHtml()}"))
                       )
                   )
                  .AddSetting(
                       SettingType.ButtonAction, "Import Colors from Clipboard", columnIndex: 2,
                       extraConfig: b => ((Button)b[0]).Pressed += () =>
                       {
                           var colors = DisplayServer.ClipboardGet().Split('|');
                           if (colors.Length < 1) return;
                           foreach (var color in colors)
                           {
                               var split = color.Split('=');
                               var id = split[0];
                               if (split.Length is not 2 || !SaveType<HexColor>.ContainsKey(id)) continue;
                               SaveType<HexColor>.Save(id, new HexColor(new Color(split[1]).ToRgba64()), true);
                           }
                       }
                   ).AddSeparator(columnIndex: 2)
                  .AddSetting(
                       SettingType.ButtonAction, "Open Emotes Directory", columnIndex: 2,
                       extraConfig: b => ((Button)b[0]).Pressed += () => OS.ShellOpen(Directories.Emotes)
                   )
                  .AddSetting(
                       SettingType.ButtonAction, "Open Portrait Directory", columnIndex: 2,
                       extraConfig: b => ((Button)b[0]).Pressed += () => OS.ShellOpen(Directories.GamePortraits)
                   )
                  .AddSeparator(columnIndex:2)
                  .AddSetting(SettingType.Text, "Sites to download portraits", columnIndex:2)
                  .AddSetting(
                       SettingType.ButtonAction, "SteamGridDB.com", columnIndex: 2,
                       extraConfig: b => ((Button)b[0]).Pressed += () => OS.ShellOpen("https://www.steamgriddb.com/")
                   )
                  .AddSetting(
                       SettingType.ButtonAction, "IDGB.com", columnIndex: 2,
                       extraConfig: b => ((Button)b[0]).Pressed += () => OS.ShellOpen("https://www.igdb.com/")
                   )
                  .AddSetting(
                       SettingType.BrowsFile, "Set Archipelago Folder", ApDir, columnIndex: 1, extraConfig:
                       f =>
                       {
                           var dialog = (FileDialog)f[0];
                           var button = (Button)f[1];
                           button.Pressed += () =>
                           {
                               var curSaved = SaveType<string>.Load(ApDir, "");
                               if (curSaved is not "") dialog.CurrentPath = curSaved;
                           };

                           dialog.DirSelected += dir => SaveType<string>.Save(ApDir, dir, true);
                       }
                   )
                  .AddSetting(
                       SettingType.SpinNumber, "Global Font Size", GlobalFontSize, 20d, 1,
                       c => ((SpinBox)c[0]).MinValue = 1
                   )
                  .AddSetting(SettingType.SpinNumber, "Text Client Font Size", TextClient.FontSizeId, 20d, 1)
        );

        LoadTheme();
    }

    public static void LoadTheme() { LoadGlobalFont(SaveType<double>.Load(GlobalFontSize, 20)); }

    public static void LoadGlobalFont(double d) => MainController.GlobalTheme.DefaultFontSize = (int)Math.Round(d);
}