using Godot;

namespace HydraTextClient.Scripts.Utility.UIHelpers;

public partial class LinkButton : Button
{
    [Export] private string Link;
    public override void _Ready() => Pressed += () => OS.ShellOpen(Link);
}