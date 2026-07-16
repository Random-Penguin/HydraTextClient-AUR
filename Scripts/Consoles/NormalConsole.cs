using System;
using System.Linq;
using System.Text;
using CreepyUtil.Archipelago;
using Godot;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.UIHelpers;

namespace HydraTextClient.Scripts.Consoles;

public partial class NormalConsole : RichTextLabel
{
    private LimitedQueue<string> Messages = new((int)SaveType<double>.Load(ChildLimiter.QueueSaveId, 200));
    private const string BLOCK = "          ";

    public override void _Ready()
    {
        AutowrapMode = TextServer.AutowrapMode.Off;
        SelectionEnabled = true;
        SaveType<double>.OnSaveEvent += (s, d) =>
        {
            if (s is not ChildLimiter.QueueSaveId) return;
            Messages.SetLimit((int)d);
        };
    }

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