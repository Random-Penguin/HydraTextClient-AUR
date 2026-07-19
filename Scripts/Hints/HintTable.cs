using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Godot;
using HydraTextClient.Scripts.Clients.TextClient.ParserEffects;
using HydraTextClient.Scripts.Connection.Slots;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Settings;
using HydraTextClient.Scripts.Settings.ItemFilter;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.UIHelpers;
using HydraTextClient.Scripts.Utility.UtilityEffects;
using static Archipelago.MultiClient.Net.Enums.HintStatus;
using static Archipelago.MultiClient.Net.Enums.ItemFlags;
using static HydraTextClient.Scripts.Utility.ColorIdConstants;

namespace HydraTextClient.Scripts.Hints;

public partial class HintTable : TextTable
{
    public const string SortOrderSaveId = "hint_table_sort";
    public override string[] EffectGroups => ["default", "hinttable"];
    public const string GlobalCopyFormatProgressive = "Theme/HintTable/CopyFormat/Progressive";
    public const string GlobalCopyFormat = "Theme/HintTable/CopyFormat";

    public override string[] Columns
        => ["", "Receiving Player", "Item", "Finding Player", "Priority", "Location", "Entrance"];

    public override long DataSize => SortedHints.Length;

    [Export] private PopupMenu HintChangePopup;
    private Hint CurrentlySelectedHint;
    private Hint[] SortedHints = [];

    public static List<SortObject> SortOrder => SaveType<List<SortObject>>.Load(SortOrderSaveId, []);

    public static Dictionary<HintStatus, int> HintStatusNumber = new()
    {
        [Priority] = 0, [Avoid] = 1, [NoPriority] = 2, [Unspecified] = 3,
        [Found] = 4,
    };

    public override void _Ready()
    {
        SaveType<HexColor>.OnSaveEvent += (id, _) =>
        {
            if (!IdToConstant.TryGetValue(id, out var constant)) return;
            if (!constant.IsPlayerColor() && !constant.IsItemColor() && constant is not (ColorConstant.FoundColor
                    or ColorConstant.NotFoundColor or ColorConstant.LocationColor)) return;
            QueueUiRefresh(false);
        };

        SaveType<string>.OnSaveEvent += (id, _) =>
        {
            if (id != PlayerEffect.SaveIdNoAlias && id != PlayerEffect.SaveIdWithAlias
                                                 && id != ItemEffect.SaveId) return;
            QueueUiRefresh(false);
        };

        SaveType<FilterType>.OnSaveEvent += (_, _) => QueueUiRefresh(true);
        SaveType<FilterType>.OnDeleteEvent += (_, _) => QueueUiRefresh(true);

        SettingsCreator.Tab(
            "Hints",
            tab =>
            {
                tab.AddLineEdit(
                    "Copy Hint Text Format (Progressive Items)", GlobalCopyFormatProgressive,
                    "{{receiver}}'s __{{item}}__ is in `{{finder}}`'s world at **{{loc}}**\\n-# {{entrance}}"
                ).AddLineEdit(
                    "Copy Hint Text Format", GlobalCopyFormat,
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
                QueueUiRefresh(true);
            };

            var mw = ConnectionController.GetCurrentMultiworld;
            mw?.Hints[slot] = client.Hints;
            QueueUiRefresh(true);
        };

        ConnectionController.DataClearCall += () =>
        {
            SortedHints = [];
            CallDeferred("clear");
        };

        SaveType<bool>.OnSaveEvent += (key, _) =>
        {
            if (!key.StartsWith("hint_table/show_")) return;
            QueueUiRefresh(true);
        };

        HintChangePopup.IndexPressed += l =>
        {
            var hint = CurrentlySelectedHint;
            if (!ConnectionController.IsConnected(hint.ReceivingPlayer)) return;
            var client = ConnectionController.GetClient(hint.ReceivingPlayer);
            if (client is null) return;

            client.UpdateHint(
                hint.FindingPlayer, hint.LocationId, l switch { 0 => Priority, 2 => Avoid, _ => NoPriority, }
            );
        };
    }

    public override void RefreshUi(bool resort)
    {
        var mw = ConnectionController.GetCurrentMultiworld;
        if (mw is null || !ConnectionController.HasLeaderClient)
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
                  .DistinctBy(hint => HashCode.Combine(
                           hint.FindingPlayer, hint.LocationId, hint.ReceivingPlayer, hint.FindingPlayer, hint.Entrance,
                           hint.ItemFlags
                       )
                   )
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
            
            orderedHints = SortOrder.Count > 0 ? SortingOrder(orderedHints, SortOrder[0], true)
                : orderedHints.ThenBy(hint => hint.ReceivingPlayer);

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
                "Item" => Order(current, hint => hint.ItemFlags.SortNumber(), option.IsDescending, isFirst),
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
            0 => $"{{{{click;Copy;{row}}}}}",
            1 or 3 => $"{{{{player;{(col is 1 ? hint.ReceivingPlayer : hint.FindingPlayer)}}}}}",
            2 => hint.GetItemEffectText(), 4 =>
                $"{{{{hintstatus;{hint.Status switch { Found => '4', NoPriority => '1', Avoid => '2', Priority => '3', _ => '0' }};{row};{ConnectionController.IsConnected(hint.ReceivingPlayer)}}}}}",
            5 => $"{{{{loc;{hint.LocationId};{hint.FindingPlayer}}}}}", 6 => $"{{{{entrance;{hint.Entrance}}}}}",
            _ => "Error",
        };
    }

    public static IOrderedEnumerable<Hint> Order(IOrderedEnumerable<Hint> arr, Func<Hint, int> compare, bool descending,
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

    public override void OnMetaClicked(string key, string[] text)
    {
        switch (key)
        {
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
                SaveType<List<SortObject>>.Save(SortOrderSaveId, order, true);
                QueueUiRefresh(true);
                break;
            case "change":
                CurrentlySelectedHint = SortedHints[int.Parse(text[0])];
                HintChangePopup.Position = Vector2I.Zero;
                HintChangePopup.Popup(new Rect2I((Vector2I)HintChangePopup.GetMousePosition(), HintChangePopup.Size));
                break;
            case TextTableClickEffect.ClickedEventMsg:
                var hint = SortedHints[int.Parse(text[0])];
                var rawCopy = SaveType<string>.Load(
                    hint.ItemFlags.HasFlag(Advancement) ? GlobalCopyFormatProgressive : GlobalCopyFormat,
                    "{{receiver}}'s __{{item}}__ is in `{{finder}}`'s world at **{{loc}}**\\n-# {{entrance}}"
                );

                DisplayServer.ClipboardSet(
                    rawCopy.CompileSimpleText(
                        new Dictionary<string, string>
                        {
                            ["finder"] = PlayerEffect.PlayerName(hint.FindingPlayer, out _),
                            ["receiver"] = PlayerEffect.PlayerName(hint.ReceivingPlayer, out _),
                            ["loc"] = hint.LocationName, ["entrance"] = hint.EntranceName, ["item"] = hint.ItemName,
                        }
                    ).Replace("\\n", "\n")
                );
                break;
        }
    }
}