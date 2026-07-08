using System;
using Godot;
using HydraTextClient.Scripts.Utility;

namespace HydraTextClient.Scripts.Clients.TextClient.ParserEffects;

public class FoundEffect : MessageParserEffect
{
    public override string Key => "found";
    public override void Effect(RichTextLabel label, string[] args, Action reloadFunction = null)
    {
        label.PushContext();
        label.PushColor(ColorIdConstants.ColorConstant.FoundColor.Color());
        label.AddText("Found");
        label.PopContext();
    }
}