using System;
using Godot;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Settings;

public static class GlobalThemeSettings
{
    public const string GlobalFontSize = "Theme/Global/FontSize";

    public static void Init()
    {
        SaveType<double>.OnSaveEvent += (s, d) =>
        {
            if (s is GlobalFontSize) LoadGlobalFont(d);
        };

        ColorIdConstants.CreateSettings();

        SettingsCreator.Tab(
            "Theme",
            tab =>
            {
                tab.AddSetting(
                    SettingType.SpinNumber, "Global Font Size", GlobalFontSize, 20d, 1,
                    c => ((SpinBox)c).MinValue = 1
                );
            }
        );
        
        LoadTheme();
    }

    public static void LoadTheme() { LoadGlobalFont(SaveType<double>.Load(GlobalFontSize, 20)); }

    public static void LoadGlobalFont(double d) => MainController.GlobalTheme.DefaultFontSize = (int)Math.Round(d);
}