using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HydraTextClient.Scripts.Clients.TextClient.ParserEffects;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;
using static HydraTextClient.Scripts.Utility.ColorIdConstants;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class DeathLinkMessage : MessageScene
{
    public const string SaveIdMessage = "Clients/TextClient/DeathLinkMessage";
    public const string DefaultMessage = "☠️ [{{groups}}] {{cause}}";

    public const string SaveIdUnknown = "Clients/TextClient/DeathLinkUnknown";
    public const string DefaultUnknown = "{{player}} Died by an Unknown cause";

    public string? LastCause;
    public string Player;
    public int PlayerSlot;
    public string Groups;

    public override void SetPacket(IMessagePacket packetBase)
    {
        if (packetBase is not DeathLinkPacket dl) return;
        if (!ConnectionController.HasLeaderClient) return;
        var leader = ConnectionController.LeaderClient!;

        Player = dl.Player;
        LastCause = dl.Cause;

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
            ["player"] = PlayerSlot is -1 ? "Unknown Player" : $"{{{{player;{PlayerSlot}}}}}",
            ["groups"] = Groups = $"{string.Join(", ", dl.Groups.Select(g => $"DeathLink{g}").ToArray())}",
        };

        if (LastCause is not null)
        {
            LastCause = LastCause.Contains(dl.Player) ? LastCause.Replace(dl.Player, "{{player}}")
                : $"{{player}} {LastCause}";

            CachedReplacement["cause"] = LastCause.CompileSimpleText(CachedReplacement);
        }

        Reload();
    }

    public override void Reload()
    {
        if (LastCause is null)
        {
            CachedReplacement["cause"] = SaveType<string>.Load(SaveIdUnknown, DefaultUnknown)
                                                         .CompileSimpleText(CachedReplacement);
        }

        UpdateFontSize(Message);

        Message.Clear();
        Message.ApplyCompiledPrintableObjs(
            SaveType<string>.Load(SaveIdMessage, DefaultMessage).CompileSimpleText(CachedReplacement)
                            .CompileRichText(GetCompileEffects(), false)
        );
    }

    public override bool CanReload(string saveId)
    {
        if (saveId is PlayerConnect) return true;
        if (IdToConstant.TryGetValue(saveId, out var constant)) return constant.IsPlayerColor();

        return saveId is SaveIdMessage or SaveIdUnknown;
    }

    public override string CopyText()
    {
        Dictionary<string, string> compile =
            new()
            {
                ["groups"] = Groups, ["player"] = PlayerSlot is -1 ? "Unknown Player"
                    : PlayerEffect.PlayerName(PlayerSlot, out _),
            };

        if (LastCause is null)
            compile["cause"] = SaveType<string>.Load(SaveIdUnknown, DefaultUnknown).CompileSimpleText(compile);
        else compile["cause"] = LastCause.CompileSimpleText(compile);
        return SaveType<string>.Load(SaveIdMessage, DefaultMessage).CompileSimpleText(compile);
    }
}