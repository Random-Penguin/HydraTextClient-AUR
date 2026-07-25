using Godot;

namespace HydraTextClient.Scripts.Utility.UIHelpers;

public partial class LinkButton : ButtonAnimation
{
    [Export] private string Link;

    public override void _Ready()
    {
        base._Ready();
        Pressed += () => OS.ShellOpen(Link);
    }
}