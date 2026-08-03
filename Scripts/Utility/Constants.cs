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
        [UiBackground] = new Color("#2a2a2a"), [PopupBackground] = new Color("#2a2a2a"),
        [TooltipColor] = Colors.Transparent, [InLogic] = Colors.ForestGreen, [NotInLogic] = Colors.DarkRed,
        [InLogicHinted] = Colors.DodgerBlue, [NotInLogicHinted] = Colors.DarkViolet,
    };

    public static Dictionary<ColorConstant, string> ConstantToId = new()
    {
        [ServerColor] = "Theme/Ap/Colors/Server", [PlayerNonConnected] = "Theme/Ap/Colors/PlayerNonConnected",
        [PlayerConnected] = "Theme/Ap/Colors/PlayerConnected", [TooltipColor] = "Theme/Ap/Colors/Tooltip",
        [PopupBackground] = "Theme/Ap/Colors/PopupBackground",
        [PlayerListedNonConnected] = "Theme/Ap/Colors/PlayerListedNonConnected",
        [LocationColor] = "Theme/Ap/Colors/LocationColor", [SpecialItemColor] = "Theme/Ap/Colors/SpecialItemColor",
        [ProgressiveItemColor] = "Theme/Ap/Colors/ProgressiveItemColor",
        [UsefulItemColor] = "Theme/Ap/Colors/UsefulItemColor", [TrapItemColor] = "Theme/Ap/Colors/TrapItemColor",
        [NormalItemColor] = "Theme/Ap/Colors/NormalItemColor", [UiBackground] = "Theme/Ap/Colors/UIBackground",
        [SpecialItemBackgroundColor] = "Theme/Ap/Colors/SpecialItemBackgroundColor",
        [ProgressiveItemBackgroundColor] = "Theme/Ap/Colors/ProgressiveItemBackgroundColor",
        [UsefulItemBackgroundColor] = "Theme/Ap/Colors/UsefulItemBackgroundColor",
        [TrapItemBackgroundColor] = "Theme/Ap/Colors/TrapItemBackgroundColor",
        [NormalItemBackgroundColor] = "Theme/Ap/Colors/NormalItemBackgroundColor",
        [NotFoundColor] = "Theme/Ap/Colors/HintNotFoundColor", [FoundColor] = "Theme/Ap/Colors/HintFoundColor",
        [Unknown] = "UnknownColorConstant", [Avoid] = "Theme/Ap/Colors/HintAvoid",
        [Priority] = "Theme/Ap/Colors/HintPriority", [Unspecified] = "Theme/Ap/Colors/HintUnspecified",
        [NoPriority] = "Theme/Ap/Colors/HintNoPriority", [EntranceColor] = "Theme/Ap/Colors/EntranceColor",
        [NotInLogic] = "Theme/Ap/Colors/OutofLogicColor", [InLogic] = "Theme/Ap/Colors/InLogicColor",
        [InLogicHinted] = "Theme/Ap/Colors/InLogicHintedColor",
        [NotInLogicHinted] = "Theme/Ap/Colors/NotInLogicHintedColor",
    };

    public static Dictionary<string, ColorConstant> IdToConstant = ConstantToId.ToDictionary(
        kv => kv.Value, kv => kv.Key
    );

    public static Dictionary<ColorConstant, string> SettingNames = new()
    {
        [ServerColor] = "Server Color", [PlayerNonConnected] = "Player (Generic)",
        [PopupBackground] = "Popup Background Color", [PlayerConnected] = "Player (Connected To)",
        [PlayerListedNonConnected] = "Player (Not Connected To)", [LocationColor] = "Location",
        [SpecialItemColor] = "Item (Special)", [ProgressiveItemColor] = "Item (Progressive)",
        [UsefulItemColor] = "Item (Useful)", [TrapItemColor] = "Item (Trap)", [NormalItemColor] = "Item (Normal)",
        [SpecialItemBackgroundColor] = "Background Item (Special)", [UiBackground] = "UI Background Color",
        [ProgressiveItemBackgroundColor] = "Background Item (Progressive)", [TooltipColor] = "Tooltip Color",
        [UsefulItemBackgroundColor] = "Background Item (Useful)", [TrapItemBackgroundColor] = "Background Item (Trap)",
        [NormalItemBackgroundColor] = "Background Item (Normal)", [NotFoundColor] = "Hint 'Not Found'",
        [FoundColor] = "Hint 'Found'", [Unknown] = "???", [Avoid] = "Hint 'Avoid'", [Priority] = "Hint 'Priority'",
        [Unspecified] = "Hint 'Unspecified'", [NoPriority] = "Hint 'No Priority'", [InLogic] = "Hint In Logic",
        [NotInLogic] = "Hint Not in Logic", [EntranceColor] = "Entrance", [InLogicHinted] = "In Logic (Hinted)",
        [NotInLogicHinted] = "Not in Logic (Hinted)",
    };

    public static void CreateSettings()
    {
        SettingsCreator.Tab(
            "Theme", tab =>
            {
                foreach (var constant in Enum.GetValues<ColorConstant>())
                {
                    if (constant is Unknown) continue;
                    tab.AddColorChanger(ConstantToId[constant], SettingNames[constant]);
                }
            }, -100
        );
    }

    public enum ColorConstant
    {
        Unknown, UiBackground, PopupBackground,
        TooltipColor, ServerColor, PlayerNonConnected,
        PlayerConnected, PlayerListedNonConnected, SpecialItemColor,
        ProgressiveItemColor, UsefulItemColor, TrapItemColor,
        NormalItemColor, SpecialItemBackgroundColor, ProgressiveItemBackgroundColor,
        UsefulItemBackgroundColor, TrapItemBackgroundColor, NormalItemBackgroundColor,
        LocationColor, EntranceColor, NotFoundColor,
        FoundColor, Priority, Unspecified,
        NoPriority, Avoid, InLogic,
        NotInLogic, InLogicHinted, NotInLogicHinted, 
    }
}