using System;
using System.Linq;
using CreepyUtil.Archipelago;
using Godot;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Utility.UIHelpers;

public partial class ChildLimiter : VBoxContainer
{
    public const string QueueSaveId = "Main/QueueHistory";
    private LimitedCollection<Control> Limiter;

    public override void _Ready()
    {
        Limiter = new LimitedCollection<Control>(
            Math.Max((int)SaveType<double>.Load(QueueSaveId, 200), 20),
            list => list.FirstOrDefault(item => !item.Visible, null)
        );
        SaveType<double>.AddIndividualEvent(QueueSaveId, SetLimit);
    }

    private void SetLimit(double d) => Limiter.SetLimit(Math.Max((int)d, 20), c => CallDeferred("RemoveTheChild", c));

    public void EmptyLimiter()
    {
        Limiter.ForEach(c => CallDeferred("RemoveTheChild", c));
        Limiter.Clear();
    }

    public void AddToLimiter(Control child)
    {
        Limiter.Add(child, c => CallDeferred("RemoveTheChild", c));
        CallDeferred("AddTheChild", child);
    }

    public void RemoveFromLimiter(Control child) => Limiter.Remove(child, c => CallDeferred("RemoveTheChild", c));
    public void AddTheChild(Control child) => AddChild(child);

    public void RemoveTheChild(Control child)
    {
        child.GetParent().RemoveChild(child);
        child.QueueFree();
    }

    public void ForEach(Action<Control> action) => Limiter.ForEach(action);

    protected override void Dispose(bool disposing) => SaveType<double>.RemoveIndividualEvent(QueueSaveId, SetLimit);
}