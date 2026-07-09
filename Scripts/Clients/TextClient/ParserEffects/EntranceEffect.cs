using System;
using Godot;
using HydraTextClient.Scripts.Utility;

namespace HydraTextClient.Scripts.Clients.TextClient.ParserEffects;

// {{entrance;entrance text}}
public partial class EntranceEffect : MessageParserEffect
{
    public override string Key => "entrance";

    public override void Effect(RichTextLabel label, string[] args, Action reloadFunction = null)
    {
        if (args.Length == 0) return;
        label.PushContext();
        label.PushColor(ColorIdConstants.ColorConstant.EntranceColor.Color());
        label.AddText(args[0]);
        label.PopContext();
    }
}