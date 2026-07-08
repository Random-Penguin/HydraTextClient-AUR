using System.Collections.Generic;
using System.Text;
using Godot;
using Godot.Collections;

namespace HydraTextClient.Scripts.Utility.UIHelpers;

public abstract partial class TextTable : RichLabelInteractions
{
    public static readonly Rect2 Zero = new(0, 0, 0, 0);
        
    public abstract string[] Columns { get; }
    public abstract long DataSize { get; }
    
    public int Padding = 0;
    public Color HeaderBgColor = new("#00000069"); 
    public Color OddBgColor = new("#00000044"); 
    public Color EvenBgColor = new("#00000000");

    public void UpdateData()
    {
        PushContext();
        PushTable(Columns.Length);

        SetCellRowBackgroundColor(HeaderBgColor, HeaderBgColor);
        for (var i = 0; i < Columns.Length; i++)
        {
            PushContext();
            PushCell();
            AddText(GetColumnText(i, this));
            PopContext();
        }

        SetCellPadding(Zero);
        SetCellRowBackgroundColor(OddBgColor, EvenBgColor);
        for (var i = 0; i < DataSize; i++)
        {
            for (var j = 0; j < Columns.Length; j++)
            {
                PushContext();
                PushCell();
                AddData(i, j, this);
                PopContext();
            }
        }

        PopContext();
    }

    public virtual string GetColumnText(int columnNum, RichTextLabel self) => Columns[columnNum];
    public abstract void AddData(int row, int col, RichTextLabel self);
    }