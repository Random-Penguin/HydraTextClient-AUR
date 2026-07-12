using System.Collections.Generic;
using Archipelago.MultiClient.Net.Packets;
using Godot;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility.Loaders;
using static HydraTextClient.Scripts.Utility.ColorIdConstants;
using static HydraTextClient.Scripts.Utility.ColorIdConstants.ColorConstant;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class ItemCheatMessage : MessageScene
{
    public const string SaveId = "Clients/TextClient/ItemCheatMessage";
    public const string Default = "{{player}} was given {{item}} from {{server}} {{loc}}";

    public override void SetPacket(IMessagePacket packetBase)
    {
        if (packetBase.GetPacket() is not ItemCheatPrintJsonPacket item) return;

        var leader = ConnectionController.LeaderClient!;
        var finder = item.Item.Player;
        var receiver = item.ReceivingPlayer;
        var itemName = leader.ItemIdToItemName(item.Item.Item, receiver);

        CachedReplacement = new Dictionary<string, string>
        {
            ["player"] = $"{{{{player;{finder}}}}}", ["server"] = "{{player;0}}",
            ["loc"] = $"{{{{loc;{item.Item.Location};{finder}}}}}",
            ["item"] = $"{{{{item;{leader.PlayerGames[receiver]};{itemName};{(int)item.Item.Flags}}}}}",
        };

        Reload();
    }

    public override void Reload()
    {
        var final = SaveType<string>.Load(SaveId, Default).CompileSimpleText(CachedReplacement);

        Message.Clear();
        Message.ApplyCompiledPrintableObjs(final.CompileRichText(GetCompileEffects(), false));
    }

    public override bool CanReload(string saveId)
    {
        if (saveId is PlayerConnect) return true;
        if (IdToConstant.TryGetValue(saveId, out var constant))
            return constant.IsPlayerColor() || constant.IsItemColor() || constant is LocationColor;

        return saveId is SaveId;
    }
}