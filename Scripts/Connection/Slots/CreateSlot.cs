using System;
using Godot;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Connection.Slots;

public partial class CreateSlot : MarginContainer
{
    [ExportGroup("Internal")]
    [Export] private LineEdit SlotName;
    [Export] private LineEdit WorldSlotName;
    [Export] private OptionButton GameImages;
    [Export] private CodeEdit SlotCommands;
    [Export] private Texture2D UnknownImage;

    public override void _Ready()
    {
        SetGameImages();
        GamePortraitLoader.OnReloadImages += SetGameImages;
        GameImages.GetPopup().AddThemeConstantOverride("icon_max_width", 14);
    }

    public SlotGameData GenSlotData() => new()
    {
        Name = SlotName.Text,
        Game = GameImages.Selected is 0 ? "Unknown" : GamePortraitLoader.GameList[GameImages.Selected - 1],
        ProcessCommands = SlotCommands.Text.Replace("\r", "").Split('\n'),
    };

    public void SetGameImages()
    {
        GameImages.Clear();
        GameImages.AddIconItem(UnknownImage, "Unknown");

        foreach (var (game, icon) in GamePortraitLoader.GetImages()) GameImages.AddIconItem(icon, game);
    }

    public void EditPortrait(string slotName)
    {
        Clear();
        var data = SaveType<SlotGameData>.Load(slotName, new SlotGameData());
        
        SlotName.Text = slotName;
        var mwName = Controllers.ConnectionController.GetMultiworldName(slotName);
        if (mwName != slotName) WorldSlotName.Text = mwName;
        
        if (GamePortraitLoader.GameList.Contains(data.Game))
        {
            GameImages.Selected = Array.IndexOf(GamePortraitLoader.GameList, data.Game) + 1;
        }
        else GameImages.Selected = 0;
        SlotCommands.Text = string.Join('\n', data.ProcessCommands);
    }
    
    public void AddOverride()
    {
        var data = GenSlotData();
        if (data.Name.Trim() is "") return;
        SaveType<SlotGameData>.Save(data.Name, data, true);
        Controllers.ConnectionController.SetMultiworldName(data.Name, WorldSlotName.Text);
        Clear();
    }

    public void Delete()
    {
        SaveType<SlotGameData>.Delete(SlotName.Text);
        Clear();
    }

    public void Clear()
    {
        SlotName.Text = "";
        WorldSlotName.Text = "";
        GameImages.Selected = 0;
        SlotCommands.Text = "";
    }
}