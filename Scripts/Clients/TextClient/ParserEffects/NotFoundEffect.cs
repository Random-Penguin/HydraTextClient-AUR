using System;
using Godot;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Clients.TextClient.ParserEffects;

public class NotFoundEffect : MessageParserEffect
{
    public static event Action? OnUpdate;
    public override string Key => "notfound";

    public override void Effect(RichTextLabel label, string[] args, Action reloadFunction = null)
    {
        label.PushContext();
        label.PushColor(ColorIdConstants.ColorConstant.NotFoundColor.Color());
        label.AddText("Not Found");
        label.PopContext();
    }

    public override void AddValueUpdater() => SaveType<HexColor>.AddIndividualEvent(
        ColorIdConstants.ColorConstant.NotFoundColor.SaveId(), _ => OnUpdate?.Invoke()
    );
}