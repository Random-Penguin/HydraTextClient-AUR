using System.Linq;
using CreepyUtil.Archipelago.ApClient;
using Godot;
using HydraTextClient.Scripts.Settings;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Utilities;

public partial class SlotUtility : HSplitContainer
{
    [Export] private PlayerInventory Inventory;
    [Export] private SearchingList ItemList;
    [Export] private SearchingList LocationList;
    private bool ShowUnobtainedItems;

    public void SetupPlayer(ApClient client)
    {
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

        ItemList.VisibilitySetter = (results, item) =>
        {
            return results.Contains(item) && (!ShowUnobtainedItems || !Inventory.RawItemNames.Contains(item));
        };
        var game = client.PlayerGames[client.PlayerSlot];
        ItemList.OnItemPressed += s => GD.Print($"clicked: [{s}]");
        ItemList.OnItemCreated += (list, index, item) => CustomAssets.ItemImage(
            game, item, game, asset =>
            {
                list.CallDeferred("set_item_icon", index, asset);
            }
        );
        ItemList.SetItems(client.Items.Select(kv => kv.Key).ToArray());
        ItemList.List.FixedIconSize = new Vector2I(fontSize, fontSize);

        LocationList.OnItemPressed += s => GD.Print($"clicked: [{s}]");
        LocationList.SetItems(
            client.Locations.Select(kv => kv.Key).Where(loc => client.MissingLocations.Contains(loc)).ToArray()
        );
    }
}