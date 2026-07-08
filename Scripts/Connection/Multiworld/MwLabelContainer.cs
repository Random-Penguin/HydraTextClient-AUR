using System.Collections.Generic;
using Godot;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Connection.Multiworld;

public partial class MwLabelContainer : VBoxContainer
{
    [Export] private MultiworldCreator Creator;
    [Export] private PackedScene DataLabel;

    public Dictionary<string, MultiworldLabel> Labels = [];

    public override void _Ready()
    {
        var mwDatas = SaveType<MultiworldData>.GetKeys();
        foreach (var data in mwDatas) CreateLabel(SaveType<MultiworldData>.Load(data, new MultiworldData()));
        SaveType<MultiworldData>.OnSaveEvent += (_, data) => CreateLabel(data);
    }

    public void CreateLabel(MultiworldData data)
    {
        if (Labels.ContainsKey(data.WorldName) || data.WorldName is "Temporary Multiworld") return;
        var label = DataLabel.Instantiate<MultiworldLabel>();
        label.MultiWorldName = data.WorldName;
        label.SetWorld += () => Creator.SetWorld(label);
        label.EditWorld += () => Creator.EditWorld(label);
        label.ClearWorld += () => Creator.ClearWorld(label);
        label.DeleteWorld += () =>
        {
            Labels.Remove(data.WorldName);
            Creator.DeleteWorld(label);
            RemoveChild(label);
            label.QueueFree();
        };
        
        AddChild(label);
        Labels[data.WorldName] = label;
    }
}