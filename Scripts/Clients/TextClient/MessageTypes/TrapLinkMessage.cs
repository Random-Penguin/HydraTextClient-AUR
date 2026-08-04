using System;
using System.Collections.Generic;
using HydraTextClient.Scripts.Clients.TextClient.ParserEffects;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class TrapLinkMessage : MessageScene
{
    public const string SaveIdMessage = "Clients/TextClient/TrapLinkMessage";
    public const string Default = "🪤 {{player}} triggered a(n) {{trap}}";
    public const string Hint = "{{player}} - player who sent the link\n{{trap}} - the trap that was triggered";
    public string Trap;
    public string Player;
    public int PlayerSlot;
    public string Copy;

    public override void SetInternalPacket(IMessagePacket packetBase)
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
            if (left < right) Player = Player[left..right];
            else PlayerSlot = -1;
        }

        if (PlayerSlot is not -1)
            PlayerSlot = leader.PlayerNames.Contains(Player) ? Array.IndexOf(leader.PlayerNames, Player) : -1;

        CachedReplacement = new Dictionary<string, string>
        {
            ["player"] = PlayerSlot is -1 ?Player : $"{{{{player;{PlayerSlot}}}}}",
            ["trap"] = Trap,
        };

        SaveType<string>.AddIndividualEvent(SaveIdMessage, CallReload);
        Reload();
    }

    public override void Reload()
    {
        UpdateFontSize(Message);
        UpdateFontSize(TimeStamp);
        Message.Clear();
        Message.ApplyCompiledPrintableObjs(
            SaveType<string>.Load(SaveIdMessage, Default).CompileSimpleText(CachedReplacement)
                            .CompileRichText(GetCompileEffects(), false)
        );
    }

    public override string CopyText() => SaveType<string>.Load(SaveIdMessage, Default).CompileSimpleText(
        new Dictionary<string, string>
        {
            ["player"] = PlayerSlot is -1 ? Player : PlayerEffect.PlayerName(PlayerSlot, true, out _),
            ["trap"] = Trap,
        }
    );

    public override void RemoveEvents() => SaveType<string>.RemoveIndividualEvent(SaveIdMessage, CallReload);
}