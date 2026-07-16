using System;
using CreepyUtil.Archipelago;
using Godot;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Utility.UIHelpers;

public partial class ChildLimiter : VBoxContainer
{
    public const string QueueSaveId = "Main/QueueHistory";
    private LimitedQueue<Control> Limiter;

    public override void _Ready()
    {
        Limiter = new LimitedQueue<Control>((int)SaveType<double>.Load(QueueSaveId, 200));
        SaveType<double>.OnSaveEvent += (s, d) =>
        {
            if (s is not QueueSaveId) return;
            Limiter.SetLimit(Math.Max((int)d, 50));
        };
    }

    public void AddToLimiter(Control child)
    {
        Limiter.Add(child, c => CallDeferred("RemoveTheChild", c));
        CallDeferred("AddTheChild", child);
    }

    public void AddTheChild(Control child) => AddChild(child);
    public void RemoveTheChild(Control child) => RemoveChild(child);
    public void ForEach(Action<Control> action) => Limiter.ForEach(action);
}