using System.Linq;
using CreepyUtil.Archipelago.ApClient;
using HydraTextClient.Scripts.Utility.UIHelpers;
using HydraTextClient.Scripts.Utility.UtilityEffects;

namespace HydraTextClient.Scripts.Clients.CircleTracker;

public partial class ProgressionItemTable : TextTable
{
    public override string[] Columns => ["", "Item", "Potential Checks"];
    public override long DataSize => OrderedData.Length;
    public TrackerPage Page;
    public (long, int)[] OrderedData = [];
    private ApClient Client => Page.Client;
    public void SetPage(TrackerPage page) => Page = page;

    public override void RefreshUi(bool recompile) =>
        OrderedData = Page.NextProgression
                          .OrderByDescending(kv => kv.Value)
                          .Select(kv => (kv.Key, kv.Value)).ToArray();

    public override string GetData(int row, int col)
    {
        var (itemId, count) = OrderedData[row];
        return col switch
        {
            0 => $"{{{{click;Hint;{row}}}}}", 1 => $"{{{{item;``{Client.PlayerGame}``;``{Client.Items[itemId]}``;1}}}}",
            2 => $"{count}", _ => "Error",
        };
    }

    public override void OnMetaClicked(string key, string[] text)
    {
        switch (key)
        {
            case TextTableClickEffect.ClickedEventMsg:
                break;
        }
    }

    public override void RunDispose(bool disposing) { }
}