using System.Collections.Generic;
using System.IO;
using Godot;
using HydraTextClient.Scripts.Clients.TextClient;
using HydraTextClient.Scripts.Hints;
using HydraTextClient.Scripts.Settings;
using HydraTextClient.Scripts.Settings.ItemFilter;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;
using Newtonsoft.Json;

namespace HydraTextClient.Scripts.Utility.Popups;

public partial class SettingsPorter : WindowSetter
{
    [Export] private CheckBox Colors;
    [Export] private CheckBox Font;
    [Export] private CheckBox Other;
    [Export] private CheckBox Slots;
    [Export] private CheckBox ItemFilters;
    private LegacyUserData OldUserData;

    public void Startup()
    {
        OldUserData = JsonConvert.DeserializeObject<LegacyUserData>(File.ReadAllText(Directories.LegacyData));
    }

    public void Apply()
    {
        if (Colors.ButtonPressed)
        {
            foreach (var (colorId, color) in OldUserData.UiColorSettings)
            {
                if (!LegacyUserData.LegacyColorIdToNewIds.TryGetValue(colorId, out var colorConst)) continue;
                colorConst.Save(color.Color);
            }
        }

        if (Font.ButtonPressed)
        {
            SaveType<double>.Save(GlobalThemeSettings.GlobalFontSize, OldUserData.GlobalFontSize, true);
            if (OldUserData.FontSizes.TryGetValue("text_client", out var textClientSize))
                SaveType<double>.Save(TextClient.FontSizeId, textClientSize, true);
            if (OldUserData.FontSizes.TryGetValue("hint_table", out var hintTableSize)) 
                SaveType<double>.Save("Theme/Ap/FontSize/HintTable", hintTableSize, true);
            if (OldUserData.FontSizes.TryGetValue("item_filter_tablet", out var itemFilterSize)) 
                SaveType<double>.Save("Theme/Ap/FontSize/ItemFilter", itemFilterSize, true);
        }
        
        if (Slots.ButtonPressed)
        {
            foreach (var (_, data) in OldUserData.GameData)
            {
                if (SaveType<SlotGameData>.ContainsKey(data.SlotName)) continue;
                SaveType<SlotGameData>.Save(data.SlotName, new SlotGameData
                {
                    Name = data.SlotName,
                    Game = data.GameName ?? "Unknown",
                }, true);
            }
        }

        if (ItemFilters.ButtonPressed)
        {
            foreach (var (_, filter) in OldUserData.ItemFilters)
            {
                var filterType = new FilterType(filter.Name, filter.Game, filter.Flags)
                {
                    ShowInHintsTable = filter.ShowInHintsTable,
                    ShowInItemLog = filter.ShowInItemLog,
                    IsSpecial = filter.IsSpecial,
                };
                SaveType<FilterType>.Save(filterType.UID, filterType, true);
            }
        }

        if (Other.ButtonPressed)
        {
            SaveType<List<SortObject>>.Save(HintTable.SortOrderSaveId, OldUserData.HintSortOrder, true);
            SaveType<bool>.Save("show_found", OldUserData.HintOptions[0], true);
            SaveType<bool>.Save("show_priority", OldUserData.HintOptions[1], true);
            SaveType<bool>.Save("show_unspecified", OldUserData.HintOptions[2], true);
            SaveType<bool>.Save("show_nopriority", OldUserData.HintOptions[3], true);
            SaveType<bool>.Save("show_avoid", OldUserData.HintOptions[4], true);
            SaveType<bool>.Save(TextClient.ShowProgressive, OldUserData.ItemLogOptions[0], true);
            SaveType<bool>.Save(TextClient.ShowUseful, OldUserData.ItemLogOptions[1], true);
            SaveType<bool>.Save(TextClient.ShowNormal, OldUserData.ItemLogOptions[2], true);
            SaveType<bool>.Save(TextClient.ShowTrap, OldUserData.ItemLogOptions[3], true);
            SaveType<bool>.Save(TextClient.ShowOnlyYou, OldUserData.ItemLogOptions[4], true);
            SaveType<bool>.Save(TextClient.ShowFoundHints, OldUserData.ShowFoundHints, true);
            SaveType<bool>.Save(GlobalThemeSettings.AlwaysOnTop, OldUserData.AlwaysOnTop, true);
            SaveType<bool>.Save(GlobalThemeSettings.DisplayNewItemsPopup, OldUserData.ShowNewItems, true);
        }
        
        SaveType<bool>.Save("Main/HasPorted", true, true);
        Close();
    }
}