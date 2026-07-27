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
    public const string HintNoAlias = "{{name}} - name of the player";

    public const string SaveIdWithAlias = "Clients/TextClient/TextEffects/PlayerWithAlias";
    public const string DefaultWithAlias = "{{alias}} ({{name}})";
    public const string HintAlias = "{{alias}} - the player's alias\n{{name}} - player slot name";

    public const string HydraAliasOverrideInCopy = "Clients/TextClient/AliasOverrideInCopy";
    public const string CopyAliasOverrideInCopy = "Clients/TextClient/CopyAliasOverrideInCopy";

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

        var player = PlayerName(playerSlot, false, out var name);

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
        ConnectionController.OnClientConnection += (_, _, _) => UpdatePlayerEffect();
        ConnectionController.OnClientLeaderChanged += (_, _) => UpdatePlayerEffect();
        ConnectionController.OnClientRemoved += (_, _, _) => UpdatePlayerEffect();
        SaveType<string>.AddIndividualEvents(_ => UpdatePlayerEffect(), SaveIdNoAlias, SaveIdWithAlias);
        SaveType<HexColor>.OnSaveEvent += (id, _) =>
        {
            if (!ColorIdConstants.IdToConstant.TryGetValue(id, out var constant)) return;
            if (constant.IsPlayerColor()) UpdatePlayerEffect();
        };
    }

    public static void UpdatePlayerEffect() => OnUpdate?.Invoke();

    public static string PlayerName(int slot, bool isCopy, out string rawName)
    {
        var mw = ConnectionController.GetCurrentMultiworld;
        if (mw is null)
        {
            rawName = "Unknown";
            return "Unknown";
        }

        var hasAlias = ConnectionController.GetPlayerInfo(slot, out rawName, out var alias, out _);

        if ((!isCopy || SaveType<bool>.Load(HydraAliasOverrideInCopy, true))
            && mw.PlayerAliases.TryGetValue(slot, out var tempAlias) && tempAlias.Trim() is not "")
        {
            alias = tempAlias;
            hasAlias = true;
        }
        if (isCopy && SaveType<bool>.Load(CopyAliasOverrideInCopy, true)
                   && mw.PlayerCopyAliases.TryGetValue(slot, out tempAlias)
                   && tempAlias.Trim() is not "") return tempAlias;

        return SaveType<string>
              .Load(
                   hasAlias ? SaveIdWithAlias : SaveIdNoAlias,
                   hasAlias ? DefaultWithAlias : DefaultNoAlias
               ).CompileSimpleText(new Dictionary<string, string> { ["alias"] = alias, ["name"] = rawName });
    }
}