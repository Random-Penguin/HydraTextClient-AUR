using System;
using Godot;
using HydraTextClient.Scripts.Clients.TextClient;
using HydraTextClient.Scripts.Utility;
using static HydraTextClient.Scripts.Utility.ColorIdConstants.ColorConstant;

namespace HydraTextClient.Scripts.Hints;

// {{hintstatus;status}}
// {{hintstatus;status;row;t/f}}
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
    public override string Group => "hinttable";
    public override string Key => "hintstatus";

    public override void Effect(RichTextLabel label, string[] args, Action reloadFunction = null)
    {
        if (args.Length is not (1 or 3)) return;
        var isClickable = args.Length is 3 && args[2].ToLower()[0] == 't';
        var status = args[0] switch
        {
            "1" => NoPriority, "2" => Avoid, "3" => Priority, "4" => FoundColor, _ => Unspecified,
        };
        var statusName = args[0] switch
        {
            "1" => "No Priority", "2" => "Avoid", "3" => "Priority", "4" => "Found", _ => "Unspecified",
        };

        label.PushContext();
        if (isClickable) label.PushMeta((string[])["change", args[1]]);
        label.PushColor(status.Color());
        label.AddText(statusName);
        label.PopContext();
    }
}