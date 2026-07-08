using System;
using CreepyUtil.Archipelago;
using Godot;

namespace HydraTextClient.Scripts.Utility.UIHelpers;

public partial class ChildLimiter : VBoxContainer
{
    [Export] private int Limit = 200;

    private LimitedQueue<Control> Limiter;

    public override void _Ready() => Limiter = new LimitedQueue<Control>(Limit);

    public void AddToLimiter(Control child)
    {
        Limiter.Add(child, c => CallDeferred("RemoveTheChild", c));
        CallDeferred("AddTheChild", child);
    }

    public void AddTheChild(Control child) => AddChild(child);
    public void RemoveTheChild(Control child) => RemoveChild(child);
    public void ForEach(Action<Control> action) => Limiter.ForEach(action);
}