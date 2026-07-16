using System;
using Godot;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility;

namespace HydraTextClient.Scripts.Clients.TextClient.ParserEffects;

// {{loc;location id;slot #}}
public class LocationEffect : MessageParserEffect
{
    public override string Key => "loc";

    public override void Effect(RichTextLabel label, string[] args, Action reloadFunction = null)
    {
        if (args.Length != 2)
        {
            label.AddText("[Invalid Location Tag]");
            return;
        }

        if (!ConnectionController.HasLeaderClient)
        {
            label.AddText("[Unknown Player]");
            return;
        }

        label.PushContext();
        label.PushColor(ColorIdConstants.ColorConstant.LocationColor.Color());
        label.AddText(LocationName(long.Parse(args[0]), int.Parse(args[1])));
        label.PopContext();
    }

    public static string LocationName(long id, int slot) => ConnectionController.LeaderClient!.LocationIdToLocationName(id, slot);
}