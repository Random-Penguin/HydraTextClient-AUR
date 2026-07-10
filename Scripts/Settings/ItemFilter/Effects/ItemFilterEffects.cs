using System;
using Godot;
using HydraTextClient.Scripts.Clients.TextClient;

namespace HydraTextClient.Scripts.Settings.ItemFilter.Effects;

// {log;t/f;row}
public class ItemFilterEffectsItemLog : MessageParserEffect
{
    public override string Group => "itemfilter";
    public override string Key => "log"; 
    public override void Effect(RichTextLabel label, string[] args, Action reloadFunction = null)
    {
        if (args.Length < 2) return;
        var state = args[0].ToLower()[0] == 't';
        label.PushContext();
        label.PushMeta((int[])[int.Parse(args[1]), 0]);
        label.PushColor(state ? Colors.Red : Colors.LimeGreen);
        label.AddText(state ? "Hide" : "Show");
        label.PopContext();
    }
}

// {table;t/f;row}
public class ItemFilterEffectsHintTable : MessageParserEffect
{
    public override string Group => "itemfilter";
    public override string Key => "table"; 
    public override void Effect(RichTextLabel label, string[] args, Action reloadFunction = null)
    {
        if (args.Length < 2) return;
        var state = args[0].ToLower()[0] == 't';
        label.PushContext();
        label.PushMeta((int[])[int.Parse(args[1]), 1]);
        label.PushColor(state ? Colors.Red : Colors.LimeGreen);
        label.AddText(state ? "Hide" : "Show");
        label.PopContext();
    }
}

// {special;t/f;row}
public class ItemFilterEffectsSpecial : MessageParserEffect
{
    public override string Group => "itemfilter";
    public override string Key => "special"; 
    public override void Effect(RichTextLabel label, string[] args, Action reloadFunction = null)
    {
        if (args.Length < 2) return;
        var state = args[0].ToLower()[0] == 't';
        label.PushContext();
        label.PushMeta((int[])[int.Parse(args[1]), 2]);
        label.PushColor(state ? Colors.Red : Colors.LimeGreen);
        label.AddText(state ? "Unmark" : "Mark");
        label.PopContext();
    }
}