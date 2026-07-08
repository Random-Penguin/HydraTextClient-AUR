using Godot;

namespace HydraTextClient.Scripts.Utility.UIHelpers;

public partial class ScrollFix : ScrollContainer
{
	private ScrollBar ScrollBar;
	private bool ToScroll;

	public override void _Ready()
	{
		ScrollBar = GetVScrollBar();
		
		ScrollBar.Changed += () =>
		{
			if (!ToScroll) return;
			ScrollToBottom();
		};
	}

	public override void _Process(double delta)
	{
		ToScroll = ScrollBar.Value >= ScrollBar.MaxValue - Size.Y;
	}

	public void ScrollToBottom()
	{
		ScrollVertical = (int)ScrollBar.MaxValue;
		ToScroll = false;
	}
}