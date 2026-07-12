using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using CreepyUtil.Archipelago.ApClient;
using Godot;
using HydraTextClient.Scripts.Settings.ItemFilter;
using HydraTextClient.Scripts.Utility.UIHelpers;

namespace HydraTextClient.Scripts.Utilities;

public partial class PlayerInventory : TextTable
{
    public override string[] Columns => ["Count", "Item", "Senders"];
    public override long DataSize => Keys.Length;
    private Dictionary<string, List<ItemInfo>> Inventory = [];
    private string[] Keys;
    private ApClient Client;
    public string[] RawItemNames;

    public void SetupInventory(ApClient client)
    {
        Client = client;
        client.ItemHandler.OnNewItemsReceived += _ => QueueUiRefresh(true);
        Client?.UpdateItemHandler();
        QueueUiRefresh(true);
    }

    public override void _PhysicsProcess(double delta) { Client?.UpdateItemHandler(); }

    public override void RefreshUi(bool recompile)
    {
        if (!recompile) return;
        Inventory = Client.ItemHandler.Items.GroupBy(item => item.UID).ToDictionary(g => g.Key, g => g.ToList());
        Keys = Inventory.OrderByDescending(kv => kv.Value.Count).Select(kv => kv.Key).ToArray();
        RawItemNames = Inventory.Values.Select(arr => arr[0].ItemName).Distinct().ToArray();
    }

    public override string GetData(int row, int col)
    {
        var items = Inventory[Keys[row]];
        return col switch { 0 => $"{items.Count}", 
            1 => $"{{{{item;{items[0].ItemGame};{items[0].ItemName};{(int)items[0].Flags}}}}}", 
            2 => $"{{{{click;View;{row}}}}}",
            _ => "Error" };
    }

    public override void OnMetaClicked(string key, string[] text) { }
}