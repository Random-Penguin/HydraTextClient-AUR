using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using Godot;
using HydraTextClient.Scripts.Clients.TextClient.ParserEffects;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.UIHelpers;
using HydraTextClient.Scripts.Utility.UtilityEffects;
using static Archipelago.MultiClient.Net.Enums.ItemFlags;
using static HydraTextClient.Scripts.Utility.ColorIdConstants;

namespace HydraTextClient.Scripts.Settings.ItemFilter;

public partial class ItemFilterDisplay : TextTable
{
    public override string[] EffectGroups => ["default", "itemfilter"];
    public static ConcurrentBag<bool> RefreshTableUi = [];
    public static Dictionary<ItemFlags, int> ItemToSortIdCache = new();
    public override string[] Columns => ["Game", "Item", "Item Log", "Hint Table", "Is Special", ""];
    public override long DataSize => SaveType<FilterType>.Count;
    private FilterType[] FilterTypes;

    public override void _Ready()
    {
        RefreshUi(true);
        SaveType<FilterType>.OnSaveEvent += (_, _) => RefreshTableUi.Add(true);
        SaveType<FilterType>.OnDeleteEvent += (_, _) => RefreshTableUi.Add(true);
        
        SaveType<HexColor>.OnSaveEvent += (id, _) =>
        {
            if (!IdToConstant.TryGetValue(id, out var constant)) return;
            if (!constant.IsItemColor()) return;
            RefreshTableUi.Add(false);
        };

        SaveType<string>.OnSaveEvent += (id, _) =>
        {
            if (id != ItemEffect.SaveId) return;
            RefreshTableUi.Add(false);
        };
    }

    public override void _Process(double delta)
    {
        if (RefreshTableUi.IsEmpty) return;
        RefreshUi(RefreshTableUi.Contains(true));
        RefreshTableUi.Clear();
    }

    public void RefreshUi(bool recompile)
    {
        FilterTypes = SaveType<FilterType>.GetValues().OrderBy(f => f.GameName).ThenBy(f => SortNumber(f.ItemFlags)).ToArray();
        GD.Print($"types: [{FilterTypes.Length}], [{recompile}]");
        UpdateData(recompile);
    }

    public override string GetData(int row, int col)
    {
        var filter = FilterTypes[row];
        return col switch
        {
            0 => filter.GameName, 1 => $"{{{{item;{filter.GameName};{filter.ItemName};{(int)filter.ItemFlags}}}}}",
            2 => $"{{{{log;{filter.ShowInItemLog};{row}}}}}", 
            3 => $"{{{{table;{filter.ShowInHintsTable};{row}}}}}",
            4 => $"{{{{special;{filter.IsSpecial};{row}}}}}", 5 => $"{{{{click;Remove;{row}}}}}", _ => "Error",
        };
    }
    
    public static int SortNumber(ItemFlags flags)
    {
        if (ItemToSortIdCache.TryGetValue(flags, out var id)) return id;
        if ((flags & Advancement) == Advancement) id = 0;
        else if ((flags & NeverExclude) == NeverExclude) id = 1;
        else if ((flags & Trap) == Trap) id = 10;
        else id = 2;
        return ItemToSortIdCache[flags] = id;
    }

    public override void OnMetaClicked(string key, string[] text)
    {
        switch (key)
        {
            case TextTableClickEffect.ClickedEventMsg:
                SaveType<FilterType>.Delete(FilterTypes[int.Parse(text[0])].UID);
                break;
        }
    }
    
    public override void OnVariantMetaClicked(Variant meta)
    {
        if (meta.VariantType is not Variant.Type.PackedInt32Array) return;
        var arr = (int[])meta;
        var filter = FilterTypes[arr[0]];
        switch (arr[1])
        {
            case 0:
                filter.ShowInItemLog = !filter.ShowInItemLog;
                break;
            case 1:
                filter.ShowInHintsTable = !filter.ShowInHintsTable;
                break;
            case 2:
                filter.IsSpecial = !filter.IsSpecial;
                break;
        }
        SaveType<FilterType>.Save(filter.UID, filter, true);
    }

    public void OpenEmptyFilter() => MainController.ShowItemFilter();
}