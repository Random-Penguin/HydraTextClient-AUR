using Godot;

namespace HydraTextClient.Scripts.Utility.Popups;

public partial class ErrorDialog : WindowSetter
{
	[Export] private RichTextLabel Msg;

	public string SetText(string text) => Msg.Text = text;
	public string AddText(string text) => Msg.Text += text;
}