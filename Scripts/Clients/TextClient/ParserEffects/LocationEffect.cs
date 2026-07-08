using System;
using Godot;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility;

namespace HydraTextClient.Scripts.Clients.TextClient.ParserEffects;

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

        var loc = ConnectionController.LeaderClient!.LocationIdToLocationName(long.Parse(args[0]), int.Parse(args[1]));
        
        label.PushContext();
        label.PushColor(ColorIdConstants.ColorConstant.LocationColor.Color());
        label.AddText(loc);
        label.PopContext();
    }
}