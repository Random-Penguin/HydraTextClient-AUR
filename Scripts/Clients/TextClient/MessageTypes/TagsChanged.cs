using System.Collections.Generic;
using Archipelago.MultiClient.Net.Packets;
using HydraTextClient.Scripts.Clients.TextClient.ParserEffects;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;
using static HydraTextClient.Scripts.Utility.ColorIdConstants;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class TagsChanged : MessageScene
{
    public const string SaveId = "Clients/TextClient/TagsChangedMessage";
    public const string Default = "{{player}} changed tags to [{{tags}}]";
    public int PlayerSlot;
    public string Tags;

    public override void SetPacket(IMessagePacket packetBase)
    {
        if (packetBase.GetPacket() is not TagsChangedPrintJsonPacket packet) return;

        CachedReplacement = new Dictionary<string, string>
        {
            ["player"] = $"{{{{player;{PlayerSlot = packet.Slot}}}}}",
            ["tags"] = Tags = string.Join(", ", packet.Tags),
        };

        Reload();
    }

    public override void Reload()
    {
        var final = SaveType<string>.Load(SaveId, Default).CompileSimpleText(CachedReplacement);

        UpdateFontSize(Message);
        Message.Clear();
        Message.ApplyCompiledPrintableObjs(final.CompileRichText(GetCompileEffects(), false));
    }

    public override bool CanReload(string saveId, out bool queueSelfForDelete)
    {
        queueSelfForDelete = false;
        if (saveId is PlayerConnect) return true;
        if (IdToConstant.TryGetValue(saveId, out var constant)) return constant.IsPlayerColor();
        return saveId is SaveId;
    }

    public override string CopyText() => SaveType<string>.Load(SaveId, Default).CompileSimpleText(
        new Dictionary<string, string> { ["player"] = PlayerEffect.PlayerName(PlayerSlot, out _), ["tags"] = Tags, }
    );
}