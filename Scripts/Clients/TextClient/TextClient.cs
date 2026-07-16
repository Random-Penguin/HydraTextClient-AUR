using System.Collections.Concurrent;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Packets;
using Godot;
using Godot.Collections;
using HydraTextClient.Scripts.Clients.TextClient.MessageTypes;
using HydraTextClient.Scripts.Clients.TextClient.ParserEffects;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Settings;
using HydraTextClient.Scripts.Settings.ItemFilter;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;
using HydraTextClient.Scripts.Utility.UIHelpers;

namespace HydraTextClient.Scripts.Clients.TextClient;

public partial class TextClient : Control
{
    public const string FontSizeId = "TextClient/FontSize";
    [Export] private Dictionary<MessageType, ChildLimiter> Containers = [];
    [Export] private Dictionary<MessageType, PackedScene> MessageScenes = [];
    [Export] private Array<ScrollFix> ScrollFixes = [];
    [Export] private Array<ChildLimiter> UniqueLimiters = [];
    [Export] private LineEdit SendMessageEdit;
    [Export] private EmotePicker EmotePicker;

    private static ConcurrentQueue<IMessagePacket> MessageQueue = [];

    public override void _Ready()
    {
        EmotePicker.EmotePicked += SendMessageEdit.AppendText;

        ConnectionController.OnClientPrepareConnection += (_, client, _, _) =>
        {
            client.ExcludeBouncedPacketsFromSelf = false;
            client.OnChatPrintPacketReceived += packet => Enqueue(MessageType.ClientMessage, packet);
            client.OnItemLogPacketReceived += packet => Enqueue(MessageType.ItemLog, packet);
            client.OnItemCheatLogPacketReceived += packet => Enqueue(MessageType.ItemCheatLog, packet);
            client.OnServerMessagePacketReceived += packet => Enqueue(MessageType.ServerMessage, packet);
            client.OnHintPrintJsonPacketReceived += packet => Enqueue(MessageType.HintMessage, packet);
            client.OnCommandResult += packet => Enqueue(MessageType.CommandResult, packet);
            client.OnJoinLogPacketReceived += packet => Enqueue(MessageType.JoinMessage, packet);
            client.OnLeaveLogPacketReceived += packet => Enqueue(MessageType.LeaveMessage, packet);
            client.OnTagsChangedLogPacketReceived += packet => Enqueue(MessageType.TagsChangedMessage, packet);
            client.OnGoalPrintJsonPacketReceived += packet => Enqueue(MessageType.GoalMessage, packet);
            client.OnDeathLinkPacketReceived += (groups, player, message)
                => MessageQueue.Enqueue(new DeathLinkPacket(groups, player, message));
            client.OnUnregisteredTrapLinkReceived
                += (player, trap) => MessageQueue.Enqueue(new TrapLinkPacket(player, trap));
        };

        SettingsCreator.Tab(
            "Text Client",
            tab =>
            {
                tab.AddLineEdit("Join Message", JoinMessage.SaveId, true, JoinMessage.Default)
                   .AddSeparator()
                   .AddLineEdit("Leave Message", LeaveMessage.SaveId, true, LeaveMessage.Default)
                   .AddSeparator()
                   .AddLineEdit("Tags Changed", TagsChanged.SaveId, true, TagsChanged.Default)
                   .AddSeparator()
                   .AddLineEdit("Goal Message", GoalMessage.SaveId, true, GoalMessage.Default)
                   .AddSeparator()
                   .AddLineEdit("Hint Message", HintMessage.SaveId, true, HintMessage.Default)
                   .AddSeparator()
                   .AddLineEdit("Trap Message", TrapLinkMessage.SaveIdMessage, true, TrapLinkMessage.Default)
                   .AddSeparator()
                   .AddLineEdit("Death Message", DeathLinkMessage.SaveIdMessage, true, DeathLinkMessage.DefaultMessage)
                   .AddSeparator()
                   .AddLineEdit(
                        "Unknown Death Cause", DeathLinkMessage.SaveIdUnknown, true, DeathLinkMessage.DefaultUnknown
                    ).AddSeparator()
                   .AddLineEdit(
                        "Item Message (Same Person)", ItemMessage.SaveIdSamePerson, true, ItemMessage.DefaultSamePerson
                    ).AddSeparator()
                   .AddLineEdit(
                        "Item Message (Different Person)", ItemMessage.SaveIdDifferentPerson, true,
                        ItemMessage.DefaultDifferentPerson
                    ).AddSeparator()
                   .AddLineEdit("Item Message (Cheated)", ItemCheatMessage.SaveId, true, ItemCheatMessage.Default)
                   .AddSeparator()
                   .AddLineEdit(
                        "Player Text (Without Alias)", PlayerEffect.SaveIdNoAlias, true, PlayerEffect.DefaultNoAlias
                    ).AddSeparator()
                   .AddLineEdit(
                        "Player Text (With Alias)", PlayerEffect.SaveIdWithAlias, true, PlayerEffect.DefaultWithAlias
                    ).AddSeparator()
                   .AddLineEdit("Item Text", ItemEffect.SaveId, true, ItemEffect.Default)
                   .AddText("Item Log Filter Options\n(Deletes Item Log Messages)", 1)
                   .AddCheckBox("Show Progressive Items", "TextClient/show_progressive", true, 1)
                   .AddCheckBox("Show Useful Items", "TextClient/show_useful", true, 1)
                   .AddCheckBox("Show Useful Items", "TextClient/show_normal", true, 1)
                   .AddCheckBox("Show Trap Items", "TextClient/show_trap", true, 1)
                   .AddCheckBox("Show Only Related to You", "TextClient/show_only_you", true, 1)
                   .AddSeparator(1)
                   .AddText("Hint Log Options", 1)
                   .AddCheckBox("Show Found Hints", "TextClient/show_found_hints", true, 1);
            }
        );

        SaveType<HexColor>.OnSaveEvent += (id, _) => ReloadUi(id);
        SaveType<string>.OnSaveEvent += (id, _) => ReloadUi(id);
        SaveType<double>.OnSaveEvent += (id, _) => ReloadUi(id);

        ConnectionController.OnClientConnection += (_, _, _) => ReloadUi(MessageScene.PlayerConnect);
        ConnectionController.OnClientRemoved += (_, _, _) => ReloadUi(MessageScene.PlayerConnect);
    }

    public override void _Process(double delta)
    {
        if (MessageQueue.IsEmpty) return;

        if (!ConnectionController.HasLeaderClient)
        {
            MessageQueue.Clear();
            return;
        }

        if (!MessageQueue.TryDequeue(out var messagePacket)) return;
        if (messagePacket.GetMsgType() is MessageType.ItemLog
            && messagePacket.GetPacket() is ItemPrintJsonPacket itemPacket)
        {
            if (SaveType<FilterType>.TryGet(itemPacket.UID, out var filter) && !filter.ShowInItemLog) return;

        }

        if (!MessageScenes.TryGetValue(messagePacket.GetMsgType(), out var scene)) return;

        var msgScene1 = scene.Instantiate<MessageScene>();
        var msgScene2 = scene.Instantiate<MessageScene>();
        msgScene1.SetPacket(messagePacket);
        msgScene2.SetPacket(messagePacket);
        msgScene1.TimeStamp.Text = messagePacket.GetTimestamp();
        msgScene2.TimeStamp.Text = messagePacket.GetTimestamp();

        if (Containers.TryGetValue(MessageType.All, out var allContainer)) allContainer.AddToLimiter(msgScene1);
        if (Containers.TryGetValue(messagePacket.GetMsgType(), out var container)) container.AddToLimiter(msgScene2);
    }

    public void Enqueue(MessageType type, ArchipelagoPacketBase packet)
        => MessageQueue.Enqueue(new MessagePacket(type, packet));

    public void SubmitMsg()
    {
        SendMessage(SendMessageEdit.Text);
        Clear("", SendMessageEdit);
    }

    public void SendMessage(string message) => ConnectionController.LeaderClient?.Say(message);
    public void Clear(string _, LineEdit edit) => edit.Clear();

    public void ScrollToBottom()
    {
        foreach (var scrollFix in ScrollFixes) scrollFix.ScrollToBottom();
    }

    public void ReloadUi(string id)
    {
        foreach (var container in UniqueLimiters)
            container.ForEach(control =>
                {
                    if (control is not MessageScene msg) return;
                    msg.ReloadUi(id);
                }
            );
    }
}

public enum MessageType
{
    All, ClientMessage, ItemLog,
    ItemCheatLog, ServerMessage, HintMessage,
    CommandResult, JoinMessage, LeaveMessage,
    TagsChangedMessage, GoalMessage, DeathLink,
    TrapLink,
}

public readonly struct DeathLinkPacket(string[] group, string player, string? cause) : IMessagePacket
{
    public readonly string[] Groups = group;
    public readonly string Player = player;
    public readonly string? Cause = cause;
    public readonly string TimeStamp = MainController.GetTimestamp();
    public MessageType GetMsgType() => MessageType.DeathLink;
    public string GetTimestamp() => TimeStamp;
    public ArchipelagoPacketBase GetPacket() => null;
}

public readonly struct TrapLinkPacket(string player, string trap) : IMessagePacket
{
    public readonly string Player = player;
    public readonly string Trap = trap;
    public readonly string TimeStamp = MainController.GetTimestamp();
    public MessageType GetMsgType() => MessageType.TrapLink;
    public string GetTimestamp() => TimeStamp;
    public ArchipelagoPacketBase GetPacket() => null;
}

public readonly struct MessagePacket(MessageType type, ArchipelagoPacketBase packet) : IMessagePacket
{
    public readonly MessageType Type = type;
    public readonly ArchipelagoPacketBase Packet = packet;
    public readonly string TimeStamp = MainController.GetTimestamp();
    public MessageType GetMsgType() => Type;
    public string GetTimestamp() => TimeStamp;
    public ArchipelagoPacketBase GetPacket() => Packet;
}

public interface IMessagePacket
{
    public MessageType GetMsgType();
    public string GetTimestamp();
    public ArchipelagoPacketBase GetPacket();
}