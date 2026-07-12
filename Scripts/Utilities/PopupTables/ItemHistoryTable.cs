using Archipelago.MultiClient.Net.Models;
using HydraTextClient.Scripts.Utility.UIHelpers;

namespace HydraTextClient.Scripts.Utilities.PopupTables;

public partial class ItemHistoryTable : TextTable
{
    public override string[] Columns { get; }
    public override long DataSize { get; }
    public override string GetData(int row, int col) => throw new System.NotImplementedException();

    public void SetItems(ItemInfo[] items)
    {
        
    }
    
    public override void RefreshUi(bool recompile)
    {
        throw new System.NotImplementedException();
    }
    
    public override void OnMetaClicked(string key, string[] text)
    {
        throw new System.NotImplementedException();
    }
}