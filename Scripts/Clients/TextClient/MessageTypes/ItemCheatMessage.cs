using System.Collections.Generic;
using Archipelago.MultiClient.Net.Packets;
using HydraTextClient.Scripts.Clients.TextClient.ParserEffects;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class ItemCheatMessage : MessageScene
{
    public const string SaveId = "Clients/TextClient/ItemCheatMessage";
    public const string Default = "{{player}} was given {{item}} from {{server}} {{loc}}";
    public int PlayerSlot;
    public string ItemName;
    public string LocationName;

    public override void SetInternalPacket(IMessagePacket packetBase)
    {
        if (packetBase.GetPacket() is not ItemCheatPrintJsonPacket item) return;
        ItemName = item.ItemName;
        LocationName = item.GetLocationName();

        CachedReplacement = new Dictionary<string, string>
        {
            ["player"] = $"{{{{player;{PlayerSlot = item.FindingPlayer}}}}}", ["server"] = "{{player;0}}",
            ["loc"] = item.GetLocationEffectText(), ["item"] = item.GetItemEffectText(),
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

    public override string CopyText() => SaveType<string>.Load(SaveId, Default).CompileSimpleText(
        new Dictionary<string, string>
        {
            ["player"] = PlayerEffect.PlayerName(PlayerSlot, out _), ["server"] = "Server", ["loc"] = LocationName,
            ["item"] = ItemName,
        }
    );

    public override void RemoveEvents() { }
}