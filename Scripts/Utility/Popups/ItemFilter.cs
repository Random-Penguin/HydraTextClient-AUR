using Archipelago.MultiClient.Net.Enums;
using Godot;
using HydraTextClient.Scripts.Settings.ItemFilter;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Utility.Popups;

public partial class ItemFilter : Window
{
    [Export] private LineEdit GameName;
    [Export] private LineEdit ItemName;
    [Export] private OptionButton ItemType;
    [Export] private CheckBox ShowInTextClient;
    [Export] private CheckBox ShowInHintTable;
    [Export] private CheckBox MarkAsSpecial;

    public void SetFilter(string game, string item, string flag)
    {
        GameName.Text = game;
        ItemName.Text = item;
        ItemType.Selected = int.Parse(flag);
    }
    
    public void CreateFilter()
    {
        FilterType filter = new(ItemName.Text, GameName.Text, (ItemFlags)ItemType.Selected)
        {
            ShowInItemLog = ShowInTextClient.ButtonPressed, ShowInHintsTable = ShowInHintTable.ButtonPressed,
            IsSpecial = MarkAsSpecial.ButtonPressed,
        };
        
        SaveType<FilterType>.Save(filter.UID, filter, true);
    }
}