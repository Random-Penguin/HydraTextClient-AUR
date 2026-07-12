using CreepyUtil.Archipelago.ApClient;
using Godot;
using HydraTextClient.Scripts.Utility.UIHelpers;

namespace HydraTextClient.Scripts.Utilities;

public partial class PlayerInventory : TextTable<PlayerInventory>
{
    public override string[] Columns { get; }
    public override long DataSize { get; }
    private ApClient Client;

    public void SetupInventory(ApClient client)
    {
        Client = client;
        // client.ItemHandler.OnNewItemsReceived += _ => QueueUiRefresh(true);
    }

    public override void _PhysicsProcess(double delta)
    {
        // Client.UpdateItemHandler();
    }

    public override void RefreshUi(bool recompile)
    {
        UpdateData(recompile);
    }
    
    public override string GetData(int row, int col) { return ""; }

    public override void OnMetaClicked(string key, string[] text) { }
}