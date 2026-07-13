using System;
using System.Linq;
using System.Text;
using CreepyUtil.Archipelago;
using Godot;

namespace HydraTextClient.Scripts.Consoles;

public partial class NormalConsole : RichTextLabel
{
    private LimitedQueue<string> Messages = new(200);
    private const string BLOCK = "          ";

    private void AddLine(string text)
    {
        Messages.Add(text);
        Text = string.Join("\n", Messages.GetQueue);
    }

    public void WriteLine(string message, bool error = false)
    {
        if (message.Length == 0) return;
        var split = message.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length == 0) return;
        
        StringBuilder sb = new();
        sb.Append(GetTimestamp()).Append("[color=").Append(error ? "red" : "white").Append(']').Append(split[0]);
        if (split.Length > 1) sb.Append('\n').Append(BLOCK).Append(string.Join($"\n{BLOCK}", split.Skip(1)));
        
        AddLine(sb.ToString());
    }

    public void WriteError(Exception err) => WriteLine($"{err.Message}\n{err.StackTrace}", true);
    public void WriteError(string err) => WriteLine(err, true); 
    
    public string GetTimestamp() => $"[color=darkgray]{DateTime.Now:[HH:mm:ss]}[/color] ";
}