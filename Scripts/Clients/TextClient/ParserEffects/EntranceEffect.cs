using System;
using Godot;
using HydraTextClient.Scripts.Utility;

namespace HydraTextClient.Scripts.Clients.TextClient.ParserEffects;

// {{entrance;entrance text}}
public class EntranceEffect : MessageParserEffect
{
    public override string Key => "entrance";

    public override void Effect(RichTextLabel label, string[] args, Action reloadFunction = null)
    {
        var entrance = args.Length == 0 ? "Vanilla" : string.Join(';', args[0]);
        label.PushContext();
        label.PushColor(ColorIdConstants.ColorConstant.EntranceColor.Color());
        label.AddText(entrance);
        label.PopContext();
    }
}