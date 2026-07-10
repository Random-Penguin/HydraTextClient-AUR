using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.UIHelpers;
using static Archipelago.MultiClient.Net.Enums.ItemFlags;

namespace HydraTextClient.Scripts.Settings.ItemFilter;

public partial class ItemFilterDisplay : TextTable
{
    public static ConcurrentBag<bool> RefreshTableUi = [];
    public static Dictionary<ItemFlags, int> ItemToSortIdCache = new();
    public override string[] Columns => ["Game", "Item", "Item Log", "Hint Table", "Is Special", ""];
    public override long DataSize => SaveType<FilterType>.Count;
    private FilterType[] FilterTypes;

    public override void _Ready()
    {
        RefreshUi(true);
        SaveType<FilterType>.OnSaveEvent += (_, _) => RefreshTableUi.Add(true);
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
        UpdateData(recompile);
    }

    public override void OnMetaClicked(string key, string[] text) { }

    public override string GetData(int row, int col)
    {
        var filter = FilterTypes[row];
        return col switch
        {
            0 => filter.GameName, 1 => $"{{{{item;{filter.GameName};{filter.ItemName};{(int)filter.ItemFlags}}}}}",
            2 => filter.ShowInItemLog ? "Hide" : "Show", 3 => filter.ShowInHintsTable ? "Hide" : "Show",
            4 => filter.IsSpecial ? "Unmark" : "Mark", 5 => "Remove", _ => "Error",
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
}