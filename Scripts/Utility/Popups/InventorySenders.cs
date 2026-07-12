using Archipelago.MultiClient.Net.Models;
using Godot;

namespace HydraTextClient.Scripts.Utility.Popups;

public partial class InventorySenders : WindowSetter
{
    [Export] private Utilities.PopupTables.SendersTable Table;

    public void SetItems(ItemInfo[] items)
    {
        Title = $"Senders of [{items[0].ItemName}]";
        Table.SetItems(items);
    }
}
