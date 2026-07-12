using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Godot;
using HydraTextClient.Scripts.Clients.TextClient;

namespace HydraTextClient.Scripts.Utility.UIHelpers;

public abstract partial class TextTable : RichLabelInteractions
{
    private ConcurrentBag<bool> QueueRefreshUi = [];
    public static readonly Rect2 Zero = new(0, 0, 0, 0);

    public virtual string[] EffectGroups => ["default"];
    public abstract string[] Columns { get; }
    public abstract long DataSize { get; }

    public int Padding = 0;
    public Color HeaderBgColor = new("#00000069");
    public Color OddBgColor = new("#00000044");
    public Color EvenBgColor = new("#00000000");
    private Dictionary<string, Action<RichTextLabel, string[]>>? CompileEffects;
    public IPrintableObj[] CompiledMessage;

    public override void _Process(double delta)
    {
        if (QueueRefreshUi.IsEmpty) return;
        var recompile = QueueRefreshUi.Contains(true);
        RefreshUi(recompile);
        UpdateData(recompile);
        QueueRefreshUi.Clear();
    }
    
    public void UpdateData(bool recompile)
    {
        Clear();
        if (DataSize == 0) return;

        if (recompile)
        {
            StringBuilder sb = new();
            sb.Append("[table=").Append(Columns.Length).Append(']');

            for (var i = 0; i < Columns.Length; i++)
                sb.Append("[cell bg=").Append(HeaderBgColor.ToHtml()).Append("] ").Append(GetColumnText(i))
                  .Append(" [/cell]");

            for (var i = 0; i < DataSize; i++)
            for (var j = 0; j < Columns.Length; j++)
            {
                var extraPadding = i % 2 == 0 ? 0 : 3;
                sb.Append(
                       i % 2 == 0 ? $"[cell bg={EvenBgColor.ToHtml()} padding=0,"
                           : $"[cell bg={OddBgColor.ToHtml()} padding=0,"
                   )
                  .Append(Padding + extraPadding)
                  .Append(",0,")
                  .Append(Padding + extraPadding)
                  .Append("] ")
                  .Append(GetData(i, j))
                  .Append(" [/cell]");
            }

            sb.Append("[/table]");
            CompiledMessage = sb.ToString().CompileRichText(GetCompileEffects(), true);
        }

        this.ApplyCompiledPrintableObjs(CompiledMessage);
    }

    public virtual Dictionary<string, Action<RichTextLabel, string[]>> GetCompileEffects()
    {
        if (CompileEffects is not null) return CompileEffects;
        return CompileEffects = MessageParser.CreateEffects(
            () => CallDeferred("UpdateData", false), ["texttable", ..EffectGroups]
        );
    }

    public virtual string GetColumnText(int columnNum) => Columns[columnNum];
    public abstract string GetData(int row, int col);
    public abstract void RefreshUi(bool recompile);
    public void QueueUiRefresh(bool recompile) => QueueRefreshUi.Add(recompile);

}