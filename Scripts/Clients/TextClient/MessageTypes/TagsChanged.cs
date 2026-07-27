using System.Collections.Generic;
using Archipelago.MultiClient.Net.Packets;
using HydraTextClient.Scripts.Clients.TextClient.ParserEffects;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class TagsChanged : MessageScene
{
    public const string SaveId = "Clients/TextClient/TagsChangedMessage";
    public const string Default = "{{player}} changed tags to [{{tags}}]";
    public const string Hint = "{{player}} - the player's tags that got changed\n{{tags}} - what the tags got changed to";
    public int PlayerSlot;
    public string Tags;

    public override void SetInternalPacket(IMessagePacket packetBase)
    {
        if (packetBase.GetPacket() is not TagsChangedPrintJsonPacket packet) return;

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
        new Dictionary<string, string> { ["player"] = PlayerEffect.PlayerName(PlayerSlot, true, out _), ["tags"] = Tags, }
    );

    public override void RemoveEvents() => SaveType<string>.RemoveIndividualEvent(SaveId, CallReload);
}