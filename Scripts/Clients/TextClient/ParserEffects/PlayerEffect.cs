using System;
using System.Collections.Generic;
using Godot;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;
using static HydraTextClient.Scripts.Utility.ColorIdConstants.ColorConstant;

// {{player;slot number}}
namespace HydraTextClient.Scripts.Clients.TextClient.ParserEffects;

public class PlayerEffect : MessageParserEffect
{
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

        var hasAlias = ConnectionController.GetPlayerInfo(playerSlot, out var name, out var alias, out _);
        
        var val = SaveType<string>
                 .Load(
                      hasAlias ? SaveIdWithAlias : SaveIdNoAlias,
                      hasAlias ? DefaultWithAlias : DefaultNoAlias
                  ).CompileRichText(
                      new Dictionary<string, Action<RichTextLabel, string[]>>
                      {
                          ["alias"] = (l, _) => l.AddText(alias), ["name"] = (l, _) => l.AddText(name),
                      }
                  );

        var color = ConnectionController.IsConnected(name)
            ? PlayerConnected.Color()
            : Connection.Slots.SlotView.ContainsSlot(name)
                ? PlayerListedNonConnected.Color()
                : PlayerNonConnected.Color();

        label.PushContext();
        label.PushHint($"player {playerSlot}");
        label.PushColor(color);
        label.ApplyCompiledPrintableObjs(val);
        label.PopContext();
    }
}