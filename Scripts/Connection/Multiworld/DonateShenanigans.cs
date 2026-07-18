using System;
using Godot;

namespace HydraTextClient.Scripts.Connection.Multiworld;

public partial class DonateShenanigans : LinkButton
{
    public string[] DonateMessages =
    [
        "Donate ❤️ >:3", "Donate ❤️ :3", "Donate ❤️ :3c=", "Donate ❤️ >:3c=", "❤ Donate ❤",
        "I need more donate messages", "Don't Donate, unless?. . .", "Donate :dancin:", "Donate?", "Donate :)",
        "Donate :)c=", "⭐ D⭐nate ⭐", "HELP IM TRAPPED! I CAN ONLY TALK THRU THIS DONATE BUTTON!",
        "Donate for more shenanigans", "Donate to support my projects", "Look ma, Im in a Donate button",
        "Donate, or else.... nothing will happen!"
    ];

    public override void _Ready() => Text = DonateMessages[Random.Shared.Next(DonateMessages.Length)];
}