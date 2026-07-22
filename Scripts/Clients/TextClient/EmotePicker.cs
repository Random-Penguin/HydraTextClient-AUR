using System.Collections.Concurrent;
using System.Linq;
using Godot;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.Popups;

namespace HydraTextClient.Scripts.Clients.TextClient;

public partial class EmotePicker : WindowSetter
{
	[Export] private LineEdit Search;
	[Export] private Container Container;
	
	[Signal] public delegate void EmotePickedEventHandler(string emote);

	private ConcurrentBag<Button> EmoteButtons = [];
	
	public override void _Ready()
	{
		UpdateEmotes();
		EmoteLoader.Singleton.OnReloadImages += UpdateEmotes;
	}

	public void PickEmote(string emote) => EmitSignalEmotePicked($"{{{{e;{emote}}}}}");

	public void UpdateSearch(string changed)
	{
		foreach (var button in EmoteButtons)
		{
			button.Visible = button.Text.Contains(changed);
		}
	}

	public void UpdateEmotes()
	{
		foreach (var button in EmoteButtons.ToArray())
		{
			Container.CallDeferred("remove_child", button);
			button.QueueFree();
		}
		
		EmoteButtons.Clear();
		var emotes = EmoteLoader.Singleton.GetImages();
		foreach (var emote in emotes.Keys.Order())
		{
			Button button = new();
			button.Icon = emotes[emote];
			button.Text = emote;
			button.VerticalIconAlignment = VerticalAlignment.Top;
			button.IconAlignment = HorizontalAlignment.Center;
			button.Pressed += () => CallDeferred("PickEmote", emote);
			button.AddThemeConstantOverride("icon_max_width", 64);
			
			Container.CallDeferred("add_child", button);
			EmoteButtons.Add(button);
		}
	}
}