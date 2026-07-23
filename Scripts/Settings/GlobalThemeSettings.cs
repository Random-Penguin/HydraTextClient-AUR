using System;
using System.Linq;
using Godot;
using HydraTextClient.Scripts.Clients.TextClient;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.UIHelpers;
using static HydraTextClient.Scripts.Controllers.MainController;
using static HydraTextClient.Scripts.Utility.ColorIdConstants;

namespace HydraTextClient.Scripts.Settings;

public static class GlobalThemeSettings
{
    public const string GlobalFontSize = "Theme/Global/FontSize";
    public const string ApDir = "Main/ArchipelagoFolder";
    public const string AlwaysOnTop = "Main/AlwaysOnTop";
    public const string DisplayNewItemsPopup = "Main/NewItemsPopupDisplay";
    public const string ClearDataOnFullDisconnect = "Main/NewItemsPopupDisplay";
    public static event Action? OnImageLoadersReload;

    public static void Init()
    {
        SetAlwaysOnTop(SaveType<bool>.Load(AlwaysOnTop, false));
        SaveType<double>.OnSaveEvent += (s, d) =>
        {
            if (s is GlobalFontSize) LoadGlobalFont(d);
        };

        CreateSettings();

        SettingsCreator.Tab(
            "Main Settings", tab =>
            {
                tab.AddBrowseFile(
                        "Set Archipelago Folder", FileDialog.FileModeEnum.OpenDir, [],
                        extraConfig: (button, dialog) =>
                        {
                            button.Pressed += () =>
                            {
                                var curSaved = SaveType<string>.Load(ApDir, "");
                                if (curSaved is not "") dialog.CurrentPath = curSaved;
                            };
                            dialog.DirSelected += dir => SaveType<string>.Save(ApDir, dir, true);
                        }
                    )
                   .AddSeparator()
                   .AddCheckBox("Clear UI on Full Disconnection", ClearDataOnFullDisconnect, true)
                   .AddSpinBox("Message History Limit", ChildLimiter.QueueSaveId, 200d)
                   .AddSeparator()
                   .AddCheckBox("Always on top", AlwaysOnTop, false, 0, b => b.Toggled += SetAlwaysOnTop)
                   .AddCheckBox("Show new Items on Connect", DisplayNewItemsPopup, true)
                   .AddButton("Force (Safety) Save", Save, 1)
                   .AddButton("Open Save Directory", () => OS.ShellOpen(Directories.MainDirectory), 1)
                   .AddSeparator(1)
                   .AddCheckBox("Check For Updates on Start", CheckForUpdate, true, 1)
                   .AddButton("Check For Updates", CheckForUpdates, 1)
                   .AddButton("Open Emotes Directory", () => OS.ShellOpen(Directories.Emotes), 2)
                   .AddButton("Open Portrait Directory", () => OS.ShellOpen(Directories.GamePortraits), 2)
                   .AddButton(
                        "Open Game Item Override Directory", () => OS.ShellOpen(Directories.GameItemImageOverrides), 2
                    )
                   .AddButton("Reload Image Loaders", () => OnImageLoadersReload?.Invoke(), 2)
                   .AddSeparator(2)
                   .AddText("Sites to download portraits", 2)
                   .AddButton("SteamGridDB.com", () => OS.ShellOpen("https://www.steamgriddb.com/"), 2)
                   .AddButton("IGDB.com", () => OS.ShellOpen("https://www.igdb.com/"), 2)
                   .AddButton(
                        "mk-404's Archipelaog Library",
                        () => OS.ShellOpen("https://mk-404.github.io/Archipelago-Games-Library/"), 2
                    );
            }, int.MinValue
        );

        SettingsCreator.Tab(
            "Theme",
            tab => tab
                  .AddButton(
                       "Export Theme Colors to Clipboard",
                       () => DisplayServer.ClipboardSet(
                           string.Join('|', ConstantToId.Select(kv => $"{kv.Value}={kv.Key.Color().ToHtml()}"))
                       ), 1
                   ).AddButton(
                       "Import Theme Colors from Clipboard"
                       , () =>
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
                       }, 1
                   )
                  .AddSeparator(1)
                  .AddBrowseFile(
                       "Set Background Image", FileDialog.FileModeEnum.OpenFile, ["*.png", "*.jpg"], col: 1,
                       extraConfig: (button, dialog) =>
                       {
                           button.Pressed += () =>
                           {
                               var curSaved = SaveType<string>.Load(WindowBackGroundImage, "");
                               if (curSaved is not "") dialog.CurrentPath = curSaved;
                           };
                           dialog.FileSelected += dir => SaveType<string>.Save(WindowBackGroundImage, dir, true);
                       }
                   ).AddSpinBox(
                       "Background Image Alpha", WindowBackGroundImageAlpha, 255d, 1, box =>
                       {
                           box.MaxValue = 255;
                           box.MinValue = 0;
                       }
                   )
                  .AddSeparator(1)
                  .AddSpinBox("Global Font Size", GlobalFontSize, 20d, 1, c => c.MinValue = 1)
                  .AddSpinBox("Text Client Font Size", TextClient.FontSizeId, 20d, 1)
        );

        LoadTheme();
    }

    public static void LoadTheme() { LoadGlobalFont(SaveType<double>.Load(GlobalFontSize, 20)); }

    public static void LoadGlobalFont(double d) => GlobalTheme.DefaultFontSize = (int)Math.Round(d);
}