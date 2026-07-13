using System;
using CreepyUtil.Archipelago.ApClient;
using Godot;
using HydraTextClient.Scripts.Utility.Popups;

namespace HydraTextClient.Scripts.Utilities.Popups;

public partial class HintPopup : WindowSetter
{
    [Export] private Label Label;
    private Action OnClick;

    public void Set(ApClient client, string title, string text, string command)
    {
        Title = title;
        Label.Text = text;
        OnClick += () => client.Say(command);
    }

    public void ConfirmClicked()
    {
        OnClick?.Invoke();
        Close();
    }
}