using System;
using Godot;
using HydraTextClient.Scripts.Clients.TextClient;

namespace HydraTextClient.Scripts.Utility.UtilityEffects;

// {{click;text;row}}
public class TextTableClickEffect : MessageParserEffect
{
    public const string ClickedEventMsg = "table row clicked";
    public override string Group => "texttable";
    public override string Key => "click";
    public override void Effect(RichTextLabel label, string[] args, Action reloadFunction = null)
    {
        if (args.Length != 2) return;
        label.PushContext();
        label.PushMeta((string[])[ClickedEventMsg, args[1], args[0]]);
        label.AddText(args[0]);
        label.PopContext();
    }
}