using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Clients.TextClient;

public static class MessageParser
{
    private static Dictionary<string, MessageParserEffect[]> Effects;

    static MessageParser()
    {
        Effects = TypeLoader.CreateTypesWithAbstractClass<MessageParserEffect>()
                            .GroupBy(effect => effect.Group)
                            .ToDictionary(g => g.Key, g => g.ToArray());

        foreach (var group in Effects.Keys)
        {
            foreach (var effect in Effects[group]) { GD.Print($"effects loaded: [{group}]:[{effect}]"); }
        }
    }

    public static Dictionary<string, Action<RichTextLabel, string[]>> CreateEffects(Action reloadFunction,
        params string[] groups)
    {
        Dictionary<string, Action<RichTextLabel, string[]>> effects = new();

        if (groups.Length == 0) groups = ["default"];

        foreach (var group in groups)
        {
            foreach (var effect in Effects[group])
            {
                effects[effect.Key] = (label, args) => effect.Effect(label, args, reloadFunction);
            }
        }

        return effects;
    }
}

public abstract class MessageParserEffect
{
    public virtual string Group => "default";
    public abstract string Key { get; }
    public abstract void Effect(RichTextLabel label, string[] args, Action? reloadFunction = null);
}

public readonly struct CallablePrintObj(Action<RichTextLabel, string[]> callable, string[] args) : IPrintableObj
{
    public void AddText(RichTextLabel label) => callable(label, args);
}

public readonly struct TextPrintObj(string text, bool append) : IPrintableObj
{
    public void AddText(RichTextLabel label)
    {
        if (append) label.AppendText(text);
        else label.AddText(text);
    }
}

public interface IPrintableObj
{
    public void AddText(RichTextLabel label);
}