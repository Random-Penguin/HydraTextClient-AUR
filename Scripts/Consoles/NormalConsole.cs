using System;
using System.IO;
using System.Linq;
using System.Text;
using CreepyUtil.Archipelago;
using Godot;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.UIHelpers;

namespace HydraTextClient.Scripts.Consoles;

public partial class NormalConsole : RichTextLabel
{
    private static StreamWriter SlotLogs = File.CreateText($"{Directories.MainDirectory}/SlotLogs.log");
    private static bool Has;
    private LimitedCollection<string> Messages = new((int)SaveType<double>.Load(ChildLimiter.QueueSaveId, 200));
    private const string BLOCK = "          ";

    public override void _Ready()
    {
        if (!Has)
        {
            SlotLogs.AutoFlush = true;
            MainController.OnExit += () => SlotLogs.Close();
            Has = true;
        }

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
        Text = string.Join("\n", Messages.GetCollection);
    }

    public void WriteLine(string message, bool error = false)
    {
        if (message.Length == 0) return;
        var split = message.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (split.Length == 0) return;

        StringBuilder sb = new();
        SlotLogs.WriteLine($"{DateTime.Now:[HH:mm:ss]} [{(error ? "ERROR" : "Info")}] [{Name}] {split[0]}");
        sb.Append(GetTimestamp()).Append("[color=").Append(error ? "red" : "white").Append(']').Append(split[0]);
        if (split.Length > 1)
        {
            sb.Append('\n').Append(BLOCK).Append(string.Join($"\n{BLOCK}", split.Skip(1)));
            SlotLogs.WriteLine($"\n{BLOCK}{string.Join($"\n{BLOCK}", split.Skip(1))}");
        }

        CallDeferred("AddLine", sb.ToString());
    }

    public void WriteError(Exception err) => WriteLine($"{err.Message}\n{err.StackTrace}", true);
    public void WriteError(string err) => WriteLine(err, true);

    public string GetTimestamp() => $"[color=darkgray]{DateTime.Now:[HH:mm:ss]}[/color] ";
}