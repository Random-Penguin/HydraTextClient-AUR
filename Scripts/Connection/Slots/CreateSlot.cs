using System;
using Godot;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.Popups;

namespace HydraTextClient.Scripts.Connection.Slots;

public partial class CreateSlot : WindowSetter
{
    [ExportGroup("Internal")]
    [Export] private LineEdit SlotName;
    [Export] private LineEdit WorldSlotName;
    [Export] private LineEdit WorldPassword;
    [Export] private OptionButton GameImages;
    [Export] private CodeEdit SlotCommands;
    [Export] private Texture2D UnknownImage;

    public override void _Ready()
    {
        SetGameImages();
        GamePortraitLoader.OnReloadImages += SetGameImages;
        GameImages.GetPopup().AddThemeConstantOverride("icon_max_width", 14);
        CloseCalled += Clear;
    }

    public SlotGameData GenSlotData() => new()
    {
        Name = SlotName.Text,
        Game = GameImages.Selected is 0 ? "Unknown" : GamePortraitLoader.GameAt(GameImages.Selected - 1),
        ProcessCommands = SlotCommands.Text.Replace("\r", "").Split('\n'),
    };

    public void SetGameImages()
    {
        GameImages.Clear();
        GameImages.AddIconItem(UnknownImage, "Unknown");

        foreach (var game in GamePortraitLoader.GameList) GameImages.AddIconItem(GamePortraitLoader.GetImage(game), game);
    }

    public void EditPortrait(string slotName)
    {
        Clear();
        var data = SaveType<SlotGameData>.Load(slotName, new SlotGameData());
        
        SlotName.Text = slotName;
        var mwName = ConnectionController.GetMultiworldName(slotName);
        if (mwName != slotName) WorldSlotName.Text = mwName;
        WorldPassword.Text = ConnectionController.GetMultiworldPassword(slotName, true);
        
        if (GamePortraitLoader.GameList.Contains(data.Game))
        {
            GameImages.Selected = Array.IndexOf(GamePortraitLoader.GameList, data.Game) + 1;
        }
        else GameImages.Selected = 0;
        SlotCommands.Text = string.Join('\n', data.ProcessCommands);
        Show();
    }

    public void AddOverride(string _) => AddOverride();
    public void AddOverride()
    {
        var data = GenSlotData();
        if (data.Name.Trim() is "") return;
        SaveType<SlotGameData>.Save(data.Name, data, true);
        ConnectionController.SetMultiworldName(data.Name, WorldSlotName.Text);
        ConnectionController.SetMultiworldPassword(data.Name, WorldPassword.Text);
        Close();
    }

    public void Delete()
    {
        SaveType<SlotGameData>.Delete(SlotName.Text);
        Close();
    }

    public void Clear()
    {
        SlotName.Text = "";
        WorldSlotName.Text = "";
        WorldPassword.Text = "";
        GameImages.Selected = 0;
        SlotCommands.Text = "";
    }
}