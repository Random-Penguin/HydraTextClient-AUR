using System;
using Godot;

namespace HydraTextClient.Scripts.Utility.Popups;

public partial class ConfirmWindow : WindowSetter
{
	[Export] private Label Label;
	[Export] private Button Yes;
	[Export] private Button No;

	public void Setup(string title, string msg, Action yes, Action? no = null)
	{
		Title = title;
		Label.Text = msg;
		Yes.Pressed += yes;
		Yes.Pressed += Close;
		if (no is null)
		{
			No.Pressed += Close;
			CallDeferred("show");
			return;
		}

		No.Pressed += no;
		CloseCalled += () => no();
		CallDeferred("show");
	}
}