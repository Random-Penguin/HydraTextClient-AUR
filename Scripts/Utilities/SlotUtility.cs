using System;
using System.Linq;
using CreepyUtil.Archipelago.ApClient;
using Godot;
using HydraTextClient.Scripts.Settings;
using HydraTextClient.Scripts.Utilities.Popups;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Utilities;

public partial class SlotUtility : HSplitContainer
{
    [Export] private PackedScene HintPopup;
    [Export] private PlayerInventory Inventory;
    [Export] private SearchingList ItemList;
    [Export] private SearchingList LocationList;
    private bool ShowUnobtainedItems;
    private ApClient Client;

    public void SetupPlayer(ApClient client)
    {
        Client = client;
        var fontSize = (int)SaveType<double>.Load(GlobalThemeSettings.GlobalFontSize, 20d);
        SaveType<double>.OnSaveEvent += (s, d) =>
        {
            if (s is not GlobalThemeSettings.GlobalFontSize) return;
            var size = (int)d;
            ItemList.List.FixedIconSize = new Vector2I(size, size);
        };

        client.OnLocationsChecked += locPack =>
        {
            LocationList.RemoveItems(
                locPack.Locations.Select(loc => client.LocationIdToLocationName(loc, client.PlayerSlot)).ToArray()
            );
        };

        Inventory.SetupInventory(client);

        ItemList.SetupBox(box =>
            {
                box.Visible = true;
                box.Text = "Show Unobtained Items";
                box.Toggled += b =>
                {
                    ShowUnobtainedItems = b;
                    ItemList.UpdateSearch(ItemList.SearchBar.Text);
                };
            }
        );

        ItemList.VisibilitySetter = (results, item) => Enumerable.Contains(results, item)
                                                       && (!ShowUnobtainedItems || !Enumerable.Contains(
                                                           Inventory.RawItemNames, item
                                                       ));

        var game = client.PlayerGames[client.PlayerSlot];
        ItemList.OnItemCreated += (list, index, item) =>
        {
            list.CallDeferred(
                "set_item_icon", index,
                CustomAssets.ItemImage(game, item, game, asset => list.CallDeferred("set_item_icon", index, asset))
            );
        };
        ItemList.SetItems(client.Items.Select(kv => kv.Key).ToArray());
        ItemList.List.FixedIconSize = new Vector2I(fontSize, fontSize);

        LocationList.SetItems(
            client.Locations.Select(kv => kv.Key).Where(loc => client.MissingLocations.Contains(loc)).ToArray()
        );

        ItemList.OnItemPressed += s => CallDeferred("CreateDialog", "Hint Item", $"Hint for\n{s}?", $"!hint {s}");
        LocationList.OnItemPressed += s => CallDeferred(
            "CreateDialog", "Hint Location", $"Hint for whats at\n{s}?", $"!hint_location {s}"
        );
    }

    public void CreateDialog(string title, string text, string command)
    {
        var popup = HintPopup.Instantiate<HintPopup>();
        popup.Set(Client, title, text, command);
        AddChild(popup);
        popup.Show();
    }
}