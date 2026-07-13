using Archipelago.MultiClient.Net.Models;
using Godot;
using HydraTextClient.Scripts.Utilities.PopupTables;
using HydraTextClient.Scripts.Utility.Popups;

namespace HydraTextClient.Scripts.Utilities.Popups;

public partial class InventorySenders : WindowSetter
{
    [Export] private SendersTable Table;

    public void SetItems(ItemInfo[] items)
    {
        Title = $"Senders of [{items[0].ItemName}]";
        Table.SetItems(items);
    }
}
