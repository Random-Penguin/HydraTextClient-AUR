using System.Collections.Concurrent;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Models;
using HydraTextClient.Scripts.Settings.ItemFilter;

namespace HydraTextClient.Scripts.Utility.DataTypes;

public class MultiworldData
{
    public string WorldName = "Untitled Multiworld";
    public string Address = "archipelago.gg";
    public string Port = "12345";
    public string Password = "";
    public string[] DeathLinkGroups = [];
    public ConcurrentDictionary<string, string> SlotNames = [];
    public ConcurrentDictionary<string, int> CheckCountsChecked = [];
    public ConcurrentDictionary<string, int> CheckCounts = [];
    public ConcurrentDictionary<string, Hint[]> Hints = [];
    public ConcurrentDictionary<string, Circle[]> Circles = [];

    public void ClearCache()
    {
        SlotNames.Clear();
        CheckCounts.Clear();
        SlotNames.Clear();
    }

    public string GetSlotName(string slot) => SlotNames.GetValueOrDefault(slot, slot);
}