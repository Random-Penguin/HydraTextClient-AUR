using System.Collections.Generic;
using Archipelago.MultiClient.Net.Packets;
using HydraTextClient.Scripts.Clients.TextClient.ParserEffects;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class JoinMessage : MessageScene
{
    public const string SaveId = "Clients/TextClient/JoinMessage";
    public const string Default = "{{player}} joined with [{{tags}}] tags";
    public int PlayerSlot;
    public string Tags;

    public override void SetInternalPacket(IMessagePacket packetBase)
    {
        if (packetBase.GetPacket() is not JoinPrintJsonPacket packet) return;

        CachedReplacement = new Dictionary<string, string>
        {
            ["player"] = $"{{{{player;{PlayerSlot = packet.Slot}}}}}",
            ["tags"] = Tags = string.Join(", ", packet.Tags),
        };

        SaveType<string>.AddIndividualEvent(SaveId, CallReload);
        Reload();
    }

    public override void Reload()
    {
        var final = SaveType<string>.Load(SaveId, Default).CompileSimpleText(CachedReplacement);

        UpdateFontSize(Message);

        Message.Clear();
        Message.ApplyCompiledPrintableObjs(final.CompileRichText(GetCompileEffects(), false));
    }

    public override string CopyText() => SaveType<string>.Load(SaveId, Default).CompileSimpleText(
        new Dictionary<string, string> { ["player"] = PlayerEffect.PlayerName(PlayerSlot, out _), ["tags"] = Tags, }
    );

    public override void RemoveEvents() => SaveType<string>.RemoveIndividualEvent(SaveId, CallReload);
}