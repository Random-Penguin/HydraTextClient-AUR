using Archipelago.MultiClient.Net.Models;
using Godot;
using HydraTextClient.Scripts.Utilities.PopupTables;
using HydraTextClient.Scripts.Utility.Popups;

namespace HydraTextClient.Scripts.Utilities.Popups;

public partial class InventoryCounter : WindowSetter
{
    [Export] private ItemCounterTable Table;
    public void SetItems(ItemInfo[] items) => Table.SetItems(items);
}