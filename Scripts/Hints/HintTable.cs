using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Godot;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Settings.ItemFilter;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.UIHelpers;
using static Archipelago.MultiClient.Net.Enums.HintStatus;
using static Archipelago.MultiClient.Net.Enums.ItemFlags;

namespace HydraTextClient.Scripts.Hints;

public partial class HintTable : TextTable
{
    public static Dictionary<ItemFlags, int> ItemToSortIdCache = new();

    public override string[] Columns
        => ["", "Receiving Player", "Item", "Finding Player", "Priority", "Location", "Entrance"];

    public override long DataSize => SortedHints.Length;

    [Export] private PopupMenu _HintChangePopup;
    private string[] _CurrentItemSelected;
    private Hint[] SortedHints = [];

    public List<SortObject> SortOrder => SaveType<List<SortObject>>.Load("hint_table_sort", []);

    public static Dictionary<HintStatus, int> HintStatusNumber = new()
    {
        [Priority] = 0, [Avoid] = 1, [NoPriority] = 2, [Unspecified] = 3,
        [Found] = 4,
    };

    public override void _Ready()
    {
        ConnectionController.OnClientConnection += (slot, client, _) =>
        {
            client.HintsTrackedEvent += hints =>
            {
                var mw = ConnectionController.GetCurrentMultiworld;
                if (mw is null) return;
                mw.Hints[slot] = hints;
                RefreshUi(true);
            };
            RefreshUi(true);
        };

        SaveType<bool>.OnSaveEvent += (key, _) =>
        {
            if (!key.StartsWith("hint_table/show_")) return;
            RefreshUi(true);
        };

        _HintChangePopup.IndexPressed += l =>
        {
            // var client = ActiveClients.First(client
            //     => client.PlayerName == PlayerSlots[int.Parse(_CurrentItemSelected[0])]);
            // client.UpdateHint(int.Parse(_CurrentItemSelected[1]), long.Parse(_CurrentItemSelected[4]), l switch
            // {
            //     0 => Priority,
            //     1 => NoPriority,
            //     2 => Avoid
            // });
        };
    }

    public void RefreshUi(bool resort)
    {
        var mw = ConnectionController.GetCurrentMultiworld;
        if (mw is null)
        {
            SortedHints = [];
            Clear();
            return;
        }

        if (resort)
        {
            var orderedHints =
                mw.Hints
                  .Where(kv => SaveType<int>.Load("hint_table/show_client", 0) switch
                       {
                           1 => Connection.Slots.SlotView.ContainsSlot(kv.Key),
                           2 => ConnectionController.IsConnected(kv.Key),
                           3 => ConnectionController.IsLeaderClient(kv.Key), _ => true
                       }
                   )
                  .SelectMany(kv => kv.Value)
                  .Where(hint =>
                       {
                           var order1 = GetOrderSlot(hint.FindingPlayer);
                           return !(GetOrderSlot(hint.FindingPlayer) == order1 && order1 == 1);
                       }
                   )
                  .Where(hint => hint.Status switch
                       {
                           Found => SaveType<bool>.Load("hint_table/show_found", false),
                           Unspecified => SaveType<bool>.Load("hint_table/show_unspecified", true),
                           NoPriority => SaveType<bool>.Load("hint_table/show_nopriority", true),
                           Avoid => SaveType<bool>.Load("hint_table/show_avoid", true),
                           Priority => SaveType<bool>.Load("hint_table/show_priority", true), _ => false,
                       }
                   )
                  .Where(hint => !SaveType<FilterType>.TryGet(hint.UID, out var filter) || filter.ShowInHintsTable)
                  .OrderBy(hint => hint.FindingPlayer)
                  .ThenBy(hint => hint.ReceivingPlayer);

            if (SortOrder.Count > 0) orderedHints = SortingOrder(orderedHints, SortOrder[0], true);

            if (SortOrder.Count > 1)
                orderedHints = SortOrder.Skip(1)
                                        .Aggregate(orderedHints, (current, option) => SortingOrder(current, option));

            SortedHints = orderedHints.ToArray();
        }
        UpdateData();
        return;

        IOrderedEnumerable<Hint> SortingOrder(IOrderedEnumerable<Hint> current, SortObject option,
            bool isFirst = false)
        {
            return option.Name switch
            {
                "Receiving Player" => Order(
                    current, hint => GetOrderSlot(hint.ReceivingPlayer),
                    option.IsDescending, isFirst
                ),
                "Item" => Order(current, hint => SortNumber(hint.ItemFlags), option.IsDescending, isFirst),
                "Finding Player" => Order(
                    current, hint => GetOrderSlot(hint.FindingPlayer), option.IsDescending,
                    isFirst
                ),
                "Priority" => Order(current, hint => HintStatusNumber[hint.Status], option.IsDescending, isFirst),
            };
        }
    }

    public override string GetColumnText(int columnNum, RichTextLabel self)
    {
        var columnText = Columns[columnNum];
        if (columnNum is 0 or > 4) return columnText;
        self.PushMeta($"sortorder_{columnText}");
        self.AddText(columnText);

        if (SortOrder.All(so => so.Name != columnText)) return " --";
        var so = SortOrder.First(so => so.Name == columnText);
        var place = SortOrder.IndexOf(so) + 1;

        return so.IsDescending ? $" {place}▼" : $" {place}▲";
    }

    public override void AddData(int row, int col, RichTextLabel self) { }

    public IOrderedEnumerable<Hint> Order(IOrderedEnumerable<Hint> arr, Func<Hint, int> compare, bool descending,
        bool first)
    {
        if (first) return !descending ? arr.OrderBy(compare) : arr.OrderByDescending(compare);
        return !descending ? arr.ThenBy(compare) : arr.ThenByDescending(compare);
    }

    public int GetOrderSlot(int slot)
    {
        var player = ConnectionController.LeaderClient!.PlayerNames[slot];
        if (ConnectionController.IsConnected(player)) return 3;
        return Connection.Slots.SlotView.ContainsSlot(player) ? 2 : 1;
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
            case "itemdialog":
                // SetAndShowItemFilterDialogue(s);
                break;
            case "sortorder":
                if (SortOrder.Any(so => so.Name == text[0]))
                {
                    var so = SortOrder.First(so => so.Name == text[0]);
                    if (so.IsDescending) SortOrder.Remove(so);
                    else so.IsDescending = true;
                }
                else
                {
                    var order = SortOrder.ToList();
                    order.Add(new SortObject(text[0]));
                    SaveType<List<SortObject>>.Save("hint_table_sort", order, true);
                }

                RefreshUi(true);
                break;
            case "change":
                _CurrentItemSelected = text;
                _HintChangePopup.Position = Vector2I.Zero;
                _HintChangePopup.Popup(
                    new Rect2I((Vector2I)_HintChangePopup.GetMousePosition(), _HintChangePopup.Size)
                );
                break;
            default: DisplayServer.ClipboardSet(text[0]); break;
        }
    }
}