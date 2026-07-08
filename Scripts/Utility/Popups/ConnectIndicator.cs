using Godot;
using static HydraTextClient.Scripts.Controllers.ConnectionController;

namespace HydraTextClient.Scripts.Utility.Popups;

public partial class ConnectIndicator : PanelContainer
{
    [Export] private Label Label;

    public override void _Process(double delta)
    {
        if (!(Visible = IsConnecting && GetConnectionCooldown <= 0)) return;
        Label.Text = $" Connecting, timer: [{GetConnectionCooldown:0.00}s]";
    }
}