using System.Collections.Concurrent;
using Archipelago.MultiClient.Net;
using Godot;
using HydraTextClient.Scripts.Clients.TextClient.ParserEffects;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Settings;
using HydraTextClient.Scripts.Utility;
using HydraTextClient.Scripts.Utility.DataTypes;
using HydraTextClient.Scripts.Utility.Loaders;

namespace HydraTextClient.Scripts.Clients.TextClient;

public partial class TextClient : Control
{
    [Export] private Godot.Collections.Dictionary<MessageType, Utility.UIHelpers.ChildLimiter> Containers = [];
    [Export] private Godot.Collections.Dictionary<MessageType, PackedScene> MessageScenes = [];
    [Export] private Godot.Collections.Array<Utility.UIHelpers.ScrollFix> ScrollFixes = [];
    [Export] private Godot.Collections.Array<Utility.UIHelpers.ChildLimiter> UniqueLimiters = [];
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
                => MessageQueue.Enqueue(new DeathlinkPacket(groups, player, message));
        };

        SettingsCreator.Tab(
            "Text Client",
            tab =>
            {
                tab.AddSetting(SettingType.Input_Submitted, "Join Message", MessageTypes.JoinMessage.SaveId, MessageTypes.JoinMessage.Default)
                   .AddSeparator()
                   .AddSetting(SettingType.Input_Submitted, "Leave Message", MessageTypes.LeaveMessage.SaveId, MessageTypes.LeaveMessage.Default)
                   .AddSeparator()
                   .AddSetting(SettingType.Input_Submitted, "Tags Changed", MessageTypes.TagsChanged.SaveId, MessageTypes.TagsChanged.Default)
                   .AddSeparator()
                   .AddSetting(SettingType.Input_Submitted, "Goal Message", MessageTypes.GoalMessage.SaveId, MessageTypes.GoalMessage.Default)
                   .AddSeparator()
                   .AddSetting(SettingType.Input_Submitted, "Hint Message", MessageTypes.HintMessage.SaveId, MessageTypes.HintMessage.Default)
                   .AddSeparator()
                   .AddSetting(
                        SettingType.Input_Submitted, "Death Message", MessageTypes.DeathLinkMessage.SaveIdMessage,
                        MessageTypes.DeathLinkMessage.DefaultMessage
                    ) // todo: forgor traplink
                   .AddSeparator()
                   .AddSetting(
                        SettingType.Input_Submitted, "Unknown Death Cause", MessageTypes.DeathLinkMessage.SaveIdUnknown,
                        MessageTypes.DeathLinkMessage.DefaultUnknown
                    )
                   .AddSeparator()
                   .AddSetting(
                        SettingType.Input_Submitted, "Item Message (Same Person)", MessageTypes.ItemMessage.SaveIdSamePerson,
                        MessageTypes.ItemMessage.DefaultSamePerson
                    ).AddSeparator()
                   .AddSetting(
                        SettingType.Input_Submitted, "Item Message (Different Person)", MessageTypes.ItemMessage.SaveIdDifferentPerson,
                        MessageTypes.ItemMessage.DefaultDifferentPerson
                    ).AddSeparator()
                   .AddSetting(
                        SettingType.Input_Submitted, "Item Message (Cheated)", MessageTypes.ItemCheatMessage.SaveId, MessageTypes.ItemCheatMessage.Default
                    ).AddSeparator()
                   .AddSetting(
                        SettingType.Input_Submitted, "Player Text (Without Alias)", PlayerEffect.SaveIdNoAlias,
                        PlayerEffect.DefaultNoAlias
                    ).AddSeparator()
                   .AddSetting(
                        SettingType.Input_Submitted, "Player Text (With Alias)", PlayerEffect.SaveIdWithAlias,
                        PlayerEffect.DefaultWithAlias
                    ).AddSeparator()
                   .AddSetting(SettingType.Input_Submitted, "Item Text", ItemEffect.SaveId, ItemEffect.Default)
                    ;
            }
        );

        SaveType<HexColor>.OnSaveEvent += (id, _) => ReloadUi(id);
        SaveType<string>.OnSaveEvent += (id, _) => ReloadUi(id);

        ConnectionController.OnClientConnection += (_, _, _) => ReloadUi(MessageTypes.MessageScene.PlayerConnect);
        ConnectionController.OnClientRemoved += (_, _, _) => ReloadUi(MessageTypes.MessageScene.PlayerConnect);
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
        if (!MessageScenes.TryGetValue(messagePacket.GetMsgType(), out var scene)) return;

        var msgScene1 = scene.Instantiate<MessageTypes.MessageScene>();
        var msgScene2 = scene.Instantiate<MessageTypes.MessageScene>();
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
                    if (control is not MessageTypes.MessageScene msg) return;
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

public readonly struct DeathlinkPacket(string[] group, string player, string? cause) : IMessagePacket
{
    public readonly string[] Groups = group;
    public readonly string Player = player;
    public readonly string? Cause = cause;
    public readonly string TimeStamp = MainController.GetTimestamp();
    public MessageType GetMsgType() => MessageType.DeathLink;
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