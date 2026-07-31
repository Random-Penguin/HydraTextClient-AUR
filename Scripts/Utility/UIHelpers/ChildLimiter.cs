using System;
using System.Collections.Concurrent;
using System.Linq;
using CreepyUtil.Archipelago;
using Godot;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Utility.UIHelpers;

public partial class ChildLimiter : VBoxContainer
{
    public const string QueueSaveId = "Main/QueueHistory";
    private ConcurrentQueue<Control> AddChildQueue = [];
    private ConcurrentQueue<Control> RemoveChildQueue = [];
    private LimitedCollection<Control> Limiter;

    public override void _Ready()
    {
        Limiter = new LimitedCollection<Control>(
            Math.Max((int)SaveType<double>.Load(QueueSaveId, 200), 20),
            list => list.FirstOrDefault(item => !item.Visible, null)
        );
        SaveType<double>.AddIndividualEvent(QueueSaveId, SetLimit);
    }

    public override void _Process(double delta)
    {
        while (!AddChildQueue.IsEmpty)
        {
            AddChildQueue.TryDequeue(out var newChild);
            AddChild(newChild);
        }

        while (!RemoveChildQueue.IsEmpty)
        {
            RemoveChildQueue.TryDequeue(out var child);
            if (child is null) continue;
            child.GetParent().RemoveChild(child);
            child.QueueFree();
        }
    }

    private void SetLimit(double d) => Limiter.SetLimit(Math.Max((int)d, 20), RemoveChildQueue.Enqueue);

    public void EmptyLimiter()
    {
        Limiter.ForEach(c => CallDeferred("RemoveTheChild", c));
        Limiter.Clear();
    }

    public void AddToLimiter(Control child)
    {
        Limiter.Add(child, RemoveChildQueue.Enqueue);
        AddChildQueue.Enqueue(child);
    }

    public void RemoveFromLimiter(Control child) => Limiter.Remove(child, RemoveChildQueue.Enqueue);
    public void ForEach(Action<Control> action) => Limiter.ForEach(action);
    protected override void Dispose(bool disposing) => SaveType<double>.RemoveIndividualEvent(QueueSaveId, SetLimit);
}