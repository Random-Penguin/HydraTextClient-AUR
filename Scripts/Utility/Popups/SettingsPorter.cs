using Godot;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Utility.Popups;

public partial class SettingsPorter : WindowSetter
{
    [Export] private CheckBox Colors;
    [Export] private CheckBox Font;
    [Export] private CheckBox Other;
    [Export] private CheckBox Slots;
    [Export] private CheckBox ItemFilters;
    
    public void Apply()
    {
        SaveType<bool>.Save("Main/HasPorted", true, true);
        Close();
    }
}