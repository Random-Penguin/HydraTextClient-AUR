using Godot;

namespace HydraTextClient.Scripts.Connection.Multiworld;

public partial class MultiworldLabel : PanelContainer
{
    [Export] private Button Set;
    [Export] private Button Edit;
    [Export] private Button ClearCache;
    [Export] private Button Delete;
    [Export] private Label WorldName;

    [Signal] public delegate void SetWorldEventHandler();
    [Signal] public delegate void EditWorldEventHandler();
    [Signal] public delegate void ClearWorldEventHandler();
    [Signal] public delegate void DeleteWorldEventHandler();

    public void EmitSetWorld() => EmitSignalSetWorld();
    public void EmitEditWorld() => EmitSignalEditWorld();
    public void EmitClearWorld() => EmitSignalClearWorld();
    public void EmitDeleteWorld() => EmitSignalDeleteWorld();
    
    public string MultiWorldName { get => WorldName.Text; set => WorldName.Text = value; }
}