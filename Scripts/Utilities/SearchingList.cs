using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace HydraTextClient.Scripts.Utilities;

public partial class SearchingList : Control
{
    [Export] private string SearchText;
    [Export] public ItemList List;
    [Export] public LineEdit SearchBar;
    [Export] private CheckBox Box;
    private FuzzySearch Search = new();
    private List<string> Items = [];
    public Func<string[], string, bool> VisibilitySetter = (searchRes, item) => searchRes.Contains(item);

    [Signal] public delegate void OnItemPressedEventHandler(string item);
    [Signal] public delegate void OnItemCreatedEventHandler(ItemList list, int index, string item);

    public override void _Ready()
    {
        SearchBar.PlaceholderText = SearchText;
        List.ItemClicked += (index, _, mouseButton) =>
        {
            if (mouseButton is not (int)MouseButton.Left) return;
            EmitSignalOnItemPressed(List.GetItemText((int)index));
        };
    }

    public void SetItems(params string[] items)
    {
        Items.Clear();
        List.Clear();
        Items = items.Distinct().Order().ToList();
        for (var i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            List.AddItem(item);
            List.SetItemSelectable(i, false);
            EmitSignalOnItemCreated(List, i, item);
        }
    }

    public void RemoveItems(params string[] items)
    {
        foreach (var item in items)
        {
            List.RemoveItem(Items.IndexOf(item));
            Items.Remove(item);
        }
    }

    public void UpdateSearch(string text)
    {
        if (text.Trim() == "")
        {
            for (var i = 0; i < List.ItemCount; i++) List.SetItemDisabled(i, false);
            return;
        }

        var results = Search.SearchAll(text, Items.ToArray()).Select(res => res.Target).ToArray();
        for (var i = 0; i < List.ItemCount; i++) List.SetItemDisabled(i, !VisibilitySetter(results, Items[i]));
    }

    public void SetupBox(Action<CheckBox> action) => action(Box);
}