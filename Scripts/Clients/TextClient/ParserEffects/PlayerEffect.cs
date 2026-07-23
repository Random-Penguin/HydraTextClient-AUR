using System;
using System.Collections.Generic;
using Godot;
using HydraTextClient.Scripts.Connection.Slots;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;
using static HydraTextClient.Scripts.Utility.ColorIdConstants.ColorConstant;

// {{player;slot number}}
namespace HydraTextClient.Scripts.Clients.TextClient.ParserEffects;

public class PlayerEffect : MessageParserEffect
{
    public static event Action? OnUpdate;
    
    public const string SaveIdNoAlias = "Clients/TextClient/TextEffects/PlayerNoAlias";
    public const string DefaultNoAlias = "{{name}}";

    public const string SaveIdWithAlias = "Clients/TextClient/TextEffects/PlayerWithAlias";
    public const string DefaultWithAlias = "{{alias}} ({{name}})";

    public override string Key => "player";

    public override void Effect(RichTextLabel label, string[] args, Action reloadFunction = null)
    {
        if (args.Length is not 1 || !int.TryParse(args[0], out var playerSlot))
        {
            label.AddText("[Invalid Player Tag]");
            return;
        }

        if (!ConnectionController.HasLeaderClient)
        {
            label.AddText("[Unknown Player]");
            return;
        }

        if (playerSlot == 0)
        {
            label.PushContext();
            label.PushHint($"player {playerSlot}");
            label.PushColor(ServerColor.Color());
            label.AddText("Server");
            label.PopContext();
            return;
        }

        var leader = ConnectionController.LeaderClient!;

        if (leader.PlayerNames.Length <= playerSlot)
        {
            label.AddText("[Not a Player]");
            return;
        }

        var player = PlayerName(playerSlot, out var name);
        
        var color = ConnectionController.IsConnected(name)
            ? PlayerConnected.Color()
            : SlotView.ContainsSlot(name)
                ? PlayerListedNonConnected.Color()
                : PlayerNonConnected.Color();

        label.PushContext();
        label.PushHint($"player {playerSlot}");
        label.PushColor(color);
        label.AddText(player);
        label.PopContext();
    }

    public override void AddValueUpdater()
    {
        ConnectionController.OnClientConnection += (_, _, _) => OnUpdate?.Invoke();
        ConnectionController.OnClientLeaderChanged += (_, _) => OnUpdate?.Invoke();
        ConnectionController.OnClientRemoved += (_, _, _) => OnUpdate?.Invoke();
        SaveType<string>.AddIndividualEvents( _ => OnUpdate?.Invoke(), SaveIdNoAlias, SaveIdWithAlias);
        SaveType<HexColor>.OnSaveEvent += (id, _) =>
        {
            if (!ColorIdConstants.IdToConstant.TryGetValue(id, out var constant)) return;
            if (constant.IsPlayerColor()) OnUpdate?.Invoke();
        };
    }

    public static string PlayerName(int slot, out string rawName)
    {
        var hasAlias = ConnectionController.GetPlayerInfo(slot, out rawName, out var alias, out _);
        return SaveType<string>
           .Load(
                hasAlias ? SaveIdWithAlias : SaveIdNoAlias,
                hasAlias ? DefaultWithAlias : DefaultNoAlias
            ).CompileSimpleText(new Dictionary<string, string> {["alias"] = alias, ["name"] = rawName});
    }
}