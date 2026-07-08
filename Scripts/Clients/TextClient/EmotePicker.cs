using System.Collections.Generic;
using System.Linq;
using Godot;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Clients.TextClient;

public partial class EmotePicker : Window
{
	[Export] private LineEdit Search;
	[Export] private Container Container;
	
	[Signal] public delegate void EmotePickedEventHandler(string emote);

	private List<Button> EmoteButtons = [];
	
	public override void _Ready()
	{
		CloseRequested += Hide;
		
		var emotes = EmoteLoader.GetImages();
		foreach (var emote in emotes.Keys.Order())
		{
			Button button = new();
			button.Icon = emotes[emote];
			button.Text = emote;
			button.VerticalIconAlignment = VerticalAlignment.Top;
			button.IconAlignment = HorizontalAlignment.Center;
			button.Pressed += () => CallDeferred("PickEmote", emote);
			button.AddThemeConstantOverride("icon_max_width", 64);
			
			Container.AddChild(button);
			EmoteButtons.Add(button);
		}
	}

	public void PickEmote(string emote) => EmitSignalEmotePicked($"{{{{e;{emote}}}}}");

	public void UpdateSearch(string changed)
	{
		foreach (var button in EmoteButtons)
		{
			button.Visible = button.Text.Contains(changed);
		}
	}
}