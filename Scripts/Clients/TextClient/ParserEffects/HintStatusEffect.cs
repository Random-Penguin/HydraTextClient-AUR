using System;
using Archipelago.MultiClient.Net.Enums;
using Godot;
using HydraTextClient.Scripts.Utility;
using static HydraTextClient.Scripts.Utility.ColorIdConstants.ColorConstant;

namespace HydraTextClient.Scripts.Clients.TextClient.ParserEffects;

// {{hintstatus;status}}
/// <summary>
/// status:
/// Unspecified = _,
/// NoPriority = 1
/// Avoid = 2
/// Priority = 3
/// Found = 4
/// </summary>
public class HintStatusEffect : MessageParserEffect
{
    public override string Key => "hintstatus";

    public override void Effect(RichTextLabel label, string[] args, Action reloadFunction = null)
    {
        var statusRaw = args.Length == 0 ? "0" : args[0];
        var status = statusRaw switch
        {
            "1" => NoPriority, "2" => Avoid, "3" => Priority, "4" => FoundColor, _ => Unspecified,
        };
        var statusName = statusRaw switch
        {
            "1" => "No Priority", "2" => "Avoid", "3" => "Priority", "4" => "Found", _ => "Unspecified",
        };

        label.PushContext();
        label.PushColor(status.Color());
        label.AddText(statusName);
        label.PopContext();
    }
}