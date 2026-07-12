
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net.Models;
using CreepyUtil.Archipelago.ApClient;
using Godot;
using HydraTextClient.Scripts.Utilities.Popups;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.UIHelpers;
using HydraTextClient.Scripts.Utility.UtilityEffects;

namespace HydraTextClient.Scripts.Utilities;

public partial class PlayerInventory : TextTable
{
    [Export] private PackedScene SendersPopup;
    [Export] private Label CheatCounter;
    public override string[] Columns => ["Count", "Item", "Senders"];
    public override long DataSize => Keys.Length;
    private Dictionary<string, ItemInfo[]> Inventory = [];
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
        var cheatedCount = Client.ItemHandler.GetCheatedItems().Length;
        CheatCounter.Visible = cheatedCount > 0;
        if (cheatedCount > 0) CheatCounter.Text = $"Cheated Items: [{cheatedCount:###,##0}]";

        Inventory = Client.ItemHandler.Items.GroupBy(item => item.UID).ToDictionary(g => g.Key, g => g.ToArray());
        Keys = Inventory.OrderBy(kv => kv.Value[0].Flags.SortNumber()).ThenByDescending(kv => kv.Value.Length)
                        .Select(kv => kv.Key).ToArray();
        RawItemNames = Inventory.Values.Select(arr => arr[0].ItemName).Distinct().ToArray();
    }

    public override string GetData(int row, int col)
    {
        var items = Inventory[Keys[row]];
        return col switch
        {
            0 => $"{items.Length}", 1 => items[0].GetEffectText(), 2 => $"{{{{click;View;{row}}}}}", _ => "Error",
        };
    }

    public override void OnMetaClicked(string key, string[] text)
    {
        switch (key)
        {
            case TextTableClickEffect.ClickedEventMsg:
                var popup = SendersPopup.Instantiate<InventorySenders>();
                popup.SetItems(Inventory[Keys[int.Parse(text[0])]]);
                AddChild(popup);
                popup.Show();
                break;
        }
    }

    public void ViewItemHistory() { }
}