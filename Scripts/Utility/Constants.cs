using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HydraTextClient.Scripts.Settings;
using static HydraTextClient.Scripts.Utility.ColorIdConstants.ColorConstant;

namespace HydraTextClient.Scripts.Utility;

public static class ColorIdConstants
{
    public static Dictionary<ColorConstant, Color> ConstantToDefaultColor = new()
    {
        [ServerColor] = Colors.Yellow, [PlayerNonConnected] = Colors.AntiqueWhite, [PlayerConnected] = Colors.Purple,
        [PlayerListedNonConnected] = Colors.MediumPurple, [LocationColor] = Colors.Green,
        [SpecialItemColor] = Colors.DarkGreen, [ProgressiveItemColor] = Colors.Goldenrod,
        [UsefulItemColor] = Colors.CornflowerBlue, [TrapItemColor] = Colors.Red, [NormalItemColor] = Colors.SlateGray,
        [SpecialItemBackgroundColor] = Colors.Transparent, [ProgressiveItemBackgroundColor] = Colors.Transparent,
        [UsefulItemBackgroundColor] = Colors.Transparent, [TrapItemBackgroundColor] = Colors.Transparent,
        [NormalItemBackgroundColor] = Colors.Transparent, [NotFoundColor] = Colors.Red, [FoundColor] = Colors.LimeGreen,
        [Unknown] = Colors.White, [Avoid] = Colors.OrangeRed, [Priority] = Colors.BlueViolet,
        [Unspecified] = Colors.NavajoWhite, [NoPriority] = Colors.CornflowerBlue, [EntranceColor] = Colors.Cyan,
    };

    public static Dictionary<ColorConstant, string> ConstantToId = new()
    {
        [ServerColor] = "Theme/Ap/Colors/Server", [PlayerNonConnected] = "Theme/Ap/Colors/PlayerNonConnected",
        [PlayerConnected] = "Theme/Ap/Colors/PlayerConnected",
        [PlayerListedNonConnected] = "Theme/Ap/Colors/PlayerListedNonConnected",
        [LocationColor] = "Theme/Ap/Colors/LocationColor", [SpecialItemColor] = "Theme/Ap/Colors/SpecialItemColor",
        [ProgressiveItemColor] = "Theme/Ap/Colors/ProgressiveItemColor",
        [UsefulItemColor] = "Theme/Ap/Colors/UsefulItemColor", [TrapItemColor] = "Theme/Ap/Colors/TrapItemColor",
        [NormalItemColor] = "Theme/Ap/Colors/NormalItemColor",
        [SpecialItemBackgroundColor] = "Theme/Ap/Colors/SpecialItemBackgroundColor",
        [ProgressiveItemBackgroundColor] = "Theme/Ap/Colors/ProgressiveItemBackgroundColor",
        [UsefulItemBackgroundColor] = "Theme/Ap/Colors/UsefulItemBackgroundColor",
        [TrapItemBackgroundColor] = "Theme/Ap/Colors/TrapItemBackgroundColor",
        [NormalItemBackgroundColor] = "Theme/Ap/Colors/NormalItemBackgroundColor",
        [NotFoundColor] = "Theme/Ap/Colors/HintNotFoundColor", [FoundColor] = "Theme/Ap/Colors/HintFoundColor",
        [Unknown] = "UnknownColorConstant", [Avoid] = "Theme/Ap/Colors/HintAvoid",
        [Priority] = "Theme/Ap/Colors/HintPriority", [Unspecified] = "Theme/Ap/Colors/HintUnspecified",
        [NoPriority] = "Theme/Ap/Colors/HintNoPriority", [EntranceColor] = "Theme/Ap/Colors/EntranceColor"
    };

    public static Dictionary<string, ColorConstant> IdToConstant = ConstantToId.ToDictionary(
        kv => kv.Value, kv => kv.Key
    );

    public static Dictionary<ColorConstant, string> SettingNames = new()
    {
        [ServerColor] = "Server Color", [PlayerNonConnected] = "Player (Generic)",
        [PlayerConnected] = "Player (Connected To)", [PlayerListedNonConnected] = "Player (Not Connected To)",
        [LocationColor] = "Location", [SpecialItemColor] = "Item (Special)",
        [ProgressiveItemColor] = "Item (Progressive)", [UsefulItemColor] = "Item (Useful)",
        [TrapItemColor] = "Item (Trap)", [NormalItemColor] = "Item (Normal)",
        [SpecialItemBackgroundColor] = "Background Item (Special)",
        [ProgressiveItemBackgroundColor] = "Background Item (Progressive)",
        [UsefulItemBackgroundColor] = "Background Item (Useful)", [TrapItemBackgroundColor] = "Background Item (Trap)",
        [NormalItemBackgroundColor] = "Background Item (Normal)", [NotFoundColor] = "Hint 'Not Found'",
        [FoundColor] = "Hint 'Found'", [Unknown] = "???", [Avoid] = "Hint 'Avoid'", [Priority] = "Hint 'Priority'",
        [Unspecified] = "Hint 'Unspecified'", [NoPriority] = "Hint 'No Priority'", [EntranceColor] = "Entrance",
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

    public enum ColorConstant
    {
        Unknown, ServerColor, PlayerNonConnected,
        PlayerConnected, PlayerListedNonConnected, SpecialItemColor,
        ProgressiveItemColor, UsefulItemColor, TrapItemColor,
        NormalItemColor, SpecialItemBackgroundColor, ProgressiveItemBackgroundColor,
        UsefulItemBackgroundColor, TrapItemBackgroundColor, NormalItemBackgroundColor,
        LocationColor, EntranceColor, NotFoundColor,
        FoundColor, Priority, Unspecified,
        NoPriority, Avoid,
    }
}