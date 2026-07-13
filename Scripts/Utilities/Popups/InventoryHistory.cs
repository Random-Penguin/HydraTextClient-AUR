using System.Collections.ObjectModel;
using Archipelago.MultiClient.Net.Models;
using Godot;
using HydraTextClient.Scripts.Utilities.PopupTables;
using HydraTextClient.Scripts.Utility.Popups;

namespace HydraTextClient.Scripts.Utilities.Popups;

public partial class InventoryHistory : WindowSetter
{
    [Export] private ItemHistoryTable Table;
    public void SetItems(ReadOnlyCollection<ItemInfo> items) => Table.SetItems(items);
}
