using Archipelago.MultiClient.Net.Enums;

namespace HydraTextClient.Scripts.Settings.ItemFilter;

public struct FilterType(string itemName, string gameName, ItemFlags itemFlags)
{
    public string ItemName = itemName;
    public string GameName = gameName;
    public ItemFlags ItemFlags = itemFlags;
    public bool ShowInHintsTable = true;
    public bool ShowInItemLog = true;
    public bool IsSpecial = false;
    public string UID => MakeUID(ItemName, GameName, ItemFlags);

    public static string MakeUID(string itemName, string gameName, ItemFlags itemFlags)
        => $"{itemName}%__%{gameName}%__%{(int)itemFlags}";

    public string GetEffectText() => $"{{{{item;{GameName};{ItemName};{(int)ItemFlags}}}}}";
}