using System;
using Godot;
using HydraTextClient.Scripts.Utility;

namespace HydraTextClient.Scripts.Clients.TextClient.ParserEffects;

public class NotFoundEffect: MessageParserEffect
{
    public override string Key => "notfound";
    public override void Effect(RichTextLabel label, string[] args, Action reloadFunction = null)
    {
        label.PushContext();
        label.PushColor(ColorIdConstants.ColorConstant.NotFoundColor.Color());
        label.AddText("Not Found");
        label.PopContext();
    }
}