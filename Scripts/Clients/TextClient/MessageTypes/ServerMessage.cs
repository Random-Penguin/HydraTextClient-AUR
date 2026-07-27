using System;
using System.Collections.Generic;
using System.Text;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using Godot;
using HydraTextClient.Scripts.Clients.TextClient.ParserEffects;
using HydraTextClient.Scripts.Controllers;
using HydraTextClient.Scripts.Utility;
using static Archipelago.MultiClient.Net.Enums.HintStatus;

namespace HydraTextClient.Scripts.Clients.TextClient.MessageTypes;

public partial class ServerMessage : AnimatedMessageScene
{
    [Export] private RichTextLabel PlayerName;
    public string Text;
    public string TheCopyText;

    public override void SetInternalPacket(IMessagePacket packetBase)
    {
        if (!ConnectionController.HasLeaderClient) return;
        var leader = ConnectionController.LeaderClient!;

        switch (packetBase.GetPacket())
        {
            case ServerChatPrintJsonPacket packet: TheCopyText = Text = packet.Message; break;
            case PrintJsonPacket cringePack:
                var parts = cringePack.Data;
                StringBuilder sb = new();
                StringBuilder copySb = new();

                foreach (var part in parts)
                {
                    switch (part.Type)
                    {
                        case JsonMessagePartType.PlayerId:
                            sb.Append("{{player;").Append(int.Parse(part.Text)).Append("}}");
                            copySb.Append(PlayerEffect.PlayerName(int.Parse(part.Text), true, out _));
                            break;
                        case JsonMessagePartType.ItemId:
                            var itemId = long.Parse(part.Text);
                            var game = leader.PlayerGames.Length <= part.Player!.Value ? "Unknown"
                                : leader.PlayerGames[part.Player!.Value];
                            var itemName = leader.ItemIdToItemName(itemId, part.Player!.Value);
                            sb.Append("{{item;``").Append(game).Append("``;``").Append(itemName).Append("``;")
                              .Append((int)part.Flags!.Value).Append("}}");
                            copySb.Append(itemName);
                            break;
                        case JsonMessagePartType.LocationId:
                            sb.Append("{{loc;").Append(part.Text).Append(';').Append(part.Player!.Value)
                              .Append("}}");
                            copySb.Append(leader.LocationIdToLocationName(long.Parse(part.Text), part.Player!.Value));
                            break;
                        case JsonMessagePartType.EntranceName:
                        {
                            sb.Append("{{entrance;").Append(part.Text.Trim()).Append("}}");
                            copySb.Append(part.Text.Trim());
                            break;
                        }
                        case JsonMessagePartType.HintStatus:
                            var status = part.HintStatus!;

                            sb.Append("{{hintstatus;").Append(
                                status switch
                                {
                                    Found => '4', NoPriority => '1', Avoid => '2', Priority => '3', _ => '0',
                                }
                            ).Append("}}");
                            copySb.Append(
                                status switch
                                {
                                    Found => "Found", NoPriority => "No Priority", Avoid => "Avoid",
                                    Priority => "Priority", _ => "Unspecified"
                                }
                            );
                            break;
                        default:
                            var text = (part.Text ?? "").Sanitize();
                            copySb.Append(text);
                            sb.Append(text);
                            break;
                    }
                }

                Text = sb.ToString();
                break;
            default: return;
        }

        CompiledMessage = Text.Sanitize().CompileRichText(GetCompileEffects(), false);
        CompiledNameMessage = "{{player;0}}".CompileRichText(GetCompileEffects(), false);

        Reload();
        RunBounceAnimation();
    }

    public override void Reload()
    {
        UpdateFontSize(Message);
        UpdateFontSize(PlayerName);

        Message.Clear();
        PlayerName.Clear();

        Message.ApplyCompiledPrintableObjs(CompiledMessage);
        PlayerName.ApplyCompiledPrintableObjs(CompiledNameMessage);
    }

    public override string CopyText() => TheCopyText;
    public override void RemoveEvents() { }

    public override Dictionary<string, Action<RichTextLabel, string[]>> GetCompileEffects()
    {
        if (CompileEffects is not null) return CompileEffects;
        return CompileEffects = MessageParser.CreateEffects(() => CallDeferred("Reload"), "default", "hinttable");
    }
}