using Godot;
using HydraTextClient.Scripts.Controllers;

namespace HydraTextClient.Scripts.Utility.UIHelpers;

public partial class PopoutWindow : Control
{
	[Export] public string Title;
	[Export] private Control Child;
	[Export] private bool RestoreSize = true;
	
	[Signal] public delegate void PoppedOutEventHandler();
	[Signal] public delegate void PoppedInEventHandler();
	
	private Window Window;
	private PanelContainer WindowContainer;
	private LayoutPreset Preset;
	private int LayoutMode;

	public override void _Ready()
	{
		Window = new Window();
		Window.Title = Title;
		Window.InitialPosition = Window.WindowInitialPosition.Absolute;
		Window.Visible = false;
		Window.WrapControls = true;
		Window.ForceNative = true;
		Window.Theme = MainController.GlobalTheme;
		Window.CloseRequested += Close;

		WindowContainer = new PanelContainer();
		WindowContainer.SetAnchorsPreset(LayoutPreset.FullRect);
			
		Window.AddChild(WindowContainer);
		AddChild(Window);

		Preset = (LayoutPreset)Child.AnchorsPreset;
		LayoutMode = Child.LayoutMode;
	}

	public void Popout()
	{
		Window.Size = (Vector2I)Size;
		Window.Position = GetViewport().GetWindow().Position;

		RemoveChild(Child);
		WindowContainer.AddChild(Child);
		
		Window.Show();
		EmitSignalPoppedOut();
	}

	public void Close()
	{
		Window.Hide();
		WindowContainer.RemoveChild(Child);
		AddChild(Child);
		MoveChild(Child, 0);

		if (RestoreSize) Child.Size = Size;
		Child.LayoutMode = LayoutMode;
		Child.SetAnchorsPreset(Preset);
		EmitSignalPoppedIn();
	}
	
	public void ToggleWindow()
	{
		if (Window.Visible) Close();
		else Popout();
	}
}