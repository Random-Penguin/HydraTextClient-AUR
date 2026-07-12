using System.Linq;
using CreepyUtil.Archipelago.ApClient;
using Godot;

namespace HydraTextClient.Scripts.Utilities;

public partial class SlotUtility : HSplitContainer
{
    [Export] private PlayerInventory Inventory;
    [Export] private SearchingList ItemList;
    [Export] private SearchingList LocationList;
    private bool ShowUnobtainedItems;

    public void SetupPlayer(ApClient client)
    {
        client.OnLocationsChecked += locPack =>
        {
            LocationList.RemoveItems(
                locPack.Locations.Select(loc => client.LocationIdToLocationName(loc, client.PlayerSlot)).ToArray()
            );
        };
        
        ItemList.SetupBox(box =>
            {
                box.Visible = true;
                box.Text = "Show Unobtained Items";
                box.Toggled += b => ShowUnobtainedItems = b;
            }
        );
        // ItemList.VisibilitySetter = (results, item) =>
        ItemList.AddItems(client.Items.Select(kv => kv.Key).ToArray());
        
        LocationList.AddItems(
            client.Locations.Select(kv => kv.Key).Where(loc => client.MissingLocations.Contains(loc)).ToArray()
        );
    }
}