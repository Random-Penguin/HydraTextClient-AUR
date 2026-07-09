using System;
using System.Collections.Generic;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility.Loaders;
using static HydraTextClient.Scripts.Utility.ColorIdConstants;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class TrapLinkMessage : MessageScene
{
    public const string SaveIdMessage = "Clients/TextClient/TrapLinkMessage";
    public const string Default = "🪤 {{player}} triggered a(n) {{trap}}";

    public string Trap;
    public string Player;
    public int PlayerSlot;

    public override void SetPacket(IMessagePacket packetBase)
    {
        if (packetBase is not TrapLinkPacket tl) return;
        if (!ConnectionController.HasLeaderClient) return;
        var leader = ConnectionController.LeaderClient!;

        Player = tl.Player;
        Trap = tl.Trap;

        if (Player.Contains('(') && Player.Contains(')'))
        {
            var left = Player.LastIndexOf('(') + 1;
            var right = Player.LastIndexOf(')');
            if (left < right) Player = Player[(left + 1)..right];
            else PlayerSlot = -1;
        }

        if (PlayerSlot is not -1)
            PlayerSlot = leader.PlayerNames.Contains(Player) ? Array.IndexOf(leader.PlayerNames, Player) : -1;

        CachedReplacement = new Dictionary<string, string>
        {
            ["player"] = PlayerSlot is -1 ? $"[hint=?]{Player}[/hint]" : $"{{{{player;{PlayerSlot}}}}}",
            ["trap"] = Trap,
        };

        Reload();
    }

    public override void Reload()
    {
        Message.Clear();
        Message.ApplyCompiledPrintableObjs(
            SaveType<string>.Load(SaveIdMessage, Default).CompileSimpleText(CachedReplacement)
                            .CompileRichText(GetCompileEffects(), false)
        );
    }

    public override bool CanReload(string saveId)
    {
        if (saveId is PlayerConnect) return true;
        if (IdToConstant.TryGetValue(saveId, out var constant)) return constant.IsPlayerColor();
        return saveId is SaveIdMessage;
    }
}