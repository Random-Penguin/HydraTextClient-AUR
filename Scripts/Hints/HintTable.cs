using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Godot;
using HydraTextClient.Scripts.Connection.Slots;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Settings;
using HydraTextClient.Scripts.Settings.ItemFilter;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.UIHelpers;
using static Archipelago.MultiClient.Net.Enums.HintStatus;
using static Archipelago.MultiClient.Net.Enums.ItemFlags;

namespace HydraTextClient.Scripts.Hints;

public partial class HintTable : TextTable
{
    public const string GlobalCopyFormat = "Theme/HintTable/CopyFormat";
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

    public static ConcurrentBag<bool> RefreshHintUi = [];

    public override void _Ready()
    {
        SettingsCreator.Tab(
            "Hints",
            tab =>
            {
                tab.AddSetting(
                    SettingType.Input_TextChange, "Copy Hint Text Format", GlobalCopyFormat,
                    "{{receiver}}'s __{{item}}__ is in `{{finder}}`'s world at **{{loc}}**\\n-# {{entrance}}"
                );
            }
        );

        ConnectionController.OnClientConnection += (slot, client, _) =>
        {
            client.HintsTrackedEvent += hints =>
            {
                var mw = ConnectionController.GetCurrentMultiworld;
                if (mw is null) return;
                mw.Hints[slot] = hints;
                RefreshHintUi.Add(true);
            };
            RefreshHintUi.Add(true);
        };

        SaveType<bool>.OnSaveEvent += (key, _) =>
        {
            if (!key.StartsWith("hint_table/show_")) return;
            RefreshHintUi.Add(true);
        };

        // _HintChangePopup.IndexPressed += l =>
        {
            // var client = ActiveClients.First(client
            //     => client.PlayerName == PlayerSlots[int.Parse(_CurrentItemSelected[0])]);
            // client.UpdateHint(int.Parse(_CurrentItemSelected[1]), long.Parse(_CurrentItemSelected[4]), l switch
            // {
            //     0 => Priority,
            //     1 => NoPriority,
            //     2 => Avoid
            // });
        }
        ;
    }

    public override void _Process(double delta)
    {
        if (RefreshHintUi.IsEmpty) return;
        RefreshUi(RefreshHintUi.Contains(true));
        RefreshHintUi.Clear();
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
                           1 => ConnectionController.IsConnected(kv.Key),
                           2 => ConnectionController.IsLeaderClient(kv.Key), _ => true,
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
                  .OrderBy(hint => hint.FindingPlayer);

            if (SortOrder.Count > 0) orderedHints = SortingOrder(orderedHints, SortOrder[0], true);
            else orderedHints = orderedHints.ThenBy(hint => hint.ReceivingPlayer);

            if (SortOrder.Count > 1)
                orderedHints = SortOrder.Skip(1)
                                        .Aggregate(orderedHints, (current, option) => SortingOrder(current, option));

            SortedHints = orderedHints.ToArray();
        }

        UpdateData(resort);
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

    public override string GetColumnText(int columnNum)
    {
        var columnText = Columns[columnNum];
        if (columnNum is 0 or > 4) return columnText;

        StringBuilder sb = new();
        sb.Append("[url=\"sortorder_").Append(columnText).Append("\"]").Append(columnText);

        if (SortOrder.All(so => so.Name != columnText))
        {
            sb.Append(" -").Append("[/url]");
            return sb.ToString();
        }

        var so = SortOrder.First(so => so.Name == columnText);
        var place = SortOrder.IndexOf(so) + 1;

        sb.Append(' ').Append(place).Append(so.IsDescending ? '▼' : '▲').Append("[/url]");
        return sb.ToString();
    }

    public override string GetData(int row, int col)
    {
        var hint = SortedHints[row];
        return col switch
        {
            0 => " ", //todo later
            1 or 3 => $"{{{{player;{(col is 1 ? hint.ReceivingPlayer : hint.FindingPlayer)}}}}}",
            2 => $"{{{{item;{hint.ItemGame};{hint.ItemName};{(int)hint.ItemFlags}}}}}", 4 =>
                $"{{{{hintstatus;{hint.Status switch { Found => '4', NoPriority => '1', Avoid => '2', Priority => '3', _ => '0' }}}}}}",
            5 => $"{{{{loc;{hint.LocationId};{hint.FindingPlayer}}}}}", 6 => $"{{{{entrance;{hint.Entrance}}}}}",
            _ => "Error",
        };
    }

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
        return SlotView.ContainsSlot(player) ? 2 : 1;
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
                var order = SortOrder.ToList();
                if (order.Any(so => so.Name == text[0]))
                {
                    var index = order.FindIndex(so => so.Name == text[0]);
                    var indexed = order[index];
                    if (indexed.IsDescending) order.RemoveAt(index);
                    else
                    {
                        indexed.IsDescending = true;
                        order[index] = indexed;
                    }
                }
                else order.Add(new SortObject(text[0]));
                SaveType<List<SortObject>>.Save("hint_table_sort", order, true);

                RefreshHintUi.Add(true);
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