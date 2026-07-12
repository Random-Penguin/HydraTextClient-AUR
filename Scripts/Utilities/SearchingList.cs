using System;
using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class SearchingList : ScrollContainer
{
    [Export] private string SearchText;
    [Export] private HFlowContainer Container;
    [Export] private LineEdit SearchBar;
    [Export] private CheckBox Box;
    private FuzzySearch Search = new();
    private Dictionary<string, Button> Items = [];
    public Func<string[], string, bool> VisibilitySetter = (searchRes, item) => searchRes.Contains(item);
    private Queue<string> ItemsToAdd = [];

    [Signal] public delegate void OnItemPressedEventHandler(string item);

    public override void _Ready() => SearchBar.PlaceholderText = SearchText;

    public override void _Process(double delta)
    {
        for (var i = 0; i < 50; i++)
        {
            if (ItemsToAdd.Count == 0) return;
            var item = ItemsToAdd.Dequeue();
            if (Items.ContainsKey(item)) continue;
            Button button = new();
            button.Text = item;
            button.Pressed += () => EmitSignalOnItemPressed(item);
            Container.AddChild(button);
            Items[item] = button;
        }
    }

    public void AddItems(params string[] items)
    {
        foreach (var item in items.Order()) ItemsToAdd.Enqueue(item);
    }

    public void RemoveItems(params string[] items)
    {
        foreach (var item in items)
        {
            Items.Remove(item, out var button);
            Container.RemoveChild(button);
            button!.QueueFree();
        }
    }

    public void UpdateSearch(string text)
    {
        if (text.Trim() == "")
        {
            foreach (var (_, button) in Items) button.Visible = true;
            return;
        }

        var results = Search.SearchAll(text, Items.Keys.ToArray()).Select(res => res.Target).ToArray();
        foreach (var (item, button) in Items) button.Visible = VisibilitySetter(results, item);
    }

    public void SetupBox(Action<CheckBox> action) => action(Box);
}