using System.Collections.Generic;
using Godot;
using HydraTextClient.Scripts.Controllers;

namespace HydraTextClient.Scripts.Utilities;

public partial class Utilities : TabContainer
{
	[Export] private PackedScene SlotUtilScene;
	private Dictionary<string, SlotUtility> UtilityPages = [];

	public override void _Ready()
	{
		ConnectionController.OnClientConnection += (name, client, _) =>
		{
			var page = UtilityPages[name] = SlotUtilScene.Instantiate<SlotUtility>();
			page.SetupPlayer(client);
			page.Name = name;
			CallDeferred("add_child", page);
		};

		ConnectionController.OnClientRemoved += (name, client, _) =>
		{

		};
	}
}