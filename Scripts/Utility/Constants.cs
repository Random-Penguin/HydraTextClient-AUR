using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HydraTextClient.Scripts.Settings;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;
using static HydraTextClient.Scripts.Utility.ColorIdConstants.ColorConstant;

namespace HydraTextClient.Scripts.Utility;

public static class ColorIdConstants
{
    public static Dictionary<ColorConstant, Color> ConstantToDefaultColor = new()
    {
        [ServerColor] = Colors.Yellow, [PlayerNonConnected] = Colors.AntiqueWhite, [PlayerConnected] = Colors.Purple,
        [PlayerListedNonConnected] = Colors.MediumPurple, [LocationColor] = Colors.Green,
        [ProgressiveItemColor] = Colors.Goldenrod, [UsefulItemColor] = Colors.CornflowerBlue,
        [TrapItemColor] = Colors.Red, [NormalItemColor] = Colors.SlateGray, [NotFoundColor] = Colors.BlueViolet,
        [FoundColor] = Colors.LimeGreen, [Unknown] = Colors.White,
    };

    public static Dictionary<ColorConstant, string> ConstantToId = new()
    {
        [ServerColor] = "Theme/Ap/Colors/Server", [PlayerNonConnected] = "Theme/Ap/Colors/PlayerNonConnected",
        [PlayerConnected] = "Theme/Ap/Colors/PlayerConnected",
        [PlayerListedNonConnected] = "Theme/Ap/Colors/PlayerListedNonConnected",
        [LocationColor] = "Theme/Ap/Colors/LocationColor",
        [ProgressiveItemColor] = "Theme/Ap/Colors/ProgressiveItemColor",
        [UsefulItemColor] = "Theme/Ap/Colors/UsefulItemColor", [TrapItemColor] = "Theme/Ap/Colors/TrapItemColor",
        [NormalItemColor] = "Theme/Ap/Colors/NormalItemColor", [NotFoundColor] = "Theme/Ap/Colors/HintNotFoundColor",
        [FoundColor] = "Theme/Ap/Colors/HintFoundColor", [Unknown] = "UnknownColorConstant",
    };

    public static Dictionary<string, ColorConstant> IdToConstant = ConstantToId.ToDictionary(
        kv => kv.Value, kv => kv.Key
    );

    public static Dictionary<ColorConstant, string> SettingNames = new()
    {
        [ServerColor] = "Server Color", [PlayerNonConnected] = "Player (Generic)",
        [PlayerConnected] = "Player (Connected To)", [PlayerListedNonConnected] = "Player (Not Connected To)",
        [LocationColor] = "Location", [ProgressiveItemColor] = "Item (Progressive)",
        [UsefulItemColor] = "Item (Useful)", [TrapItemColor] = "Item (Trap)", [NormalItemColor] = "Item (Normal)",
        [NotFoundColor] = "Hint 'Not Found'", [FoundColor] = "Hint 'Found'", [Unknown] = "???",
    };

    public static void CreateSettings()
    {
        SettingsCreator.Tab(
            "Theme", tab =>
            {
                foreach (var constant in Enum.GetValues<ColorConstant>())
                {
                    if (constant is Unknown) continue;
                    tab.AddSetting(
                        SettingType.HexColor, SettingNames[constant], ConstantToId[constant],
                        ConstantToDefaultColor[constant]
                    );
                }
            }, int.MinValue
        );
    }

    public static Color Color(this ColorConstant constant) => SaveType<HexColor>.Load(
        ConstantToId[constant], ConstantToDefaultColor[constant]
    );

    public static Color Color(this string constant) => IdToConstant.GetValueOrDefault(constant, Unknown).Color();

    public static void Save(this ColorConstant constant, Color color, bool broadcast = true)
        => SaveType<HexColor>.Save(ConstantToId[constant], color, broadcast);

    public static bool IsPlayerColor(this ColorConstant constant) => constant is PlayerConnected
        or PlayerListedNonConnected or PlayerNonConnected or ServerColor;

    public static bool IsItemColor(this ColorConstant constant) => constant is NormalItemColor
        or ProgressiveItemColor or TrapItemColor or UsefulItemColor;

    public enum ColorConstant
    {
        Unknown, ServerColor, PlayerNonConnected,
        PlayerConnected, PlayerListedNonConnected, ProgressiveItemColor,
        UsefulItemColor, TrapItemColor, NormalItemColor,
        LocationColor, NotFoundColor, FoundColor
    }
}