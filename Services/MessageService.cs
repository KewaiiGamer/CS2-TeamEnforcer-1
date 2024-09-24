using System.Text;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using TeamEnforcer.Helpers;

namespace TeamEnforcer.Services;

public class MessageService(string pluginPrefix)
{
    public Dictionary<MsgType, char> messageColors = new()
    {
        { MsgType.Normal, ChatColors.Default},
        { MsgType.Warning, ChatColors.Yellow},
        { MsgType.Error, ChatColors.Red }
    };
    
    private readonly string _pluginPrefix = pluginPrefix;

    public void PrintMessage(CCSPlayerController? player, string message, MsgType type = MsgType.Normal)
    {
        if (player == null || !player.IsReal()) return;

        
        var fullMessage = new StringBuilder(_pluginPrefix)
            .Append($" {messageColors.GetValueOrDefault(type, ChatColors.Default)}")
            .Append(message)
            .Append($"{ChatColors.Default}");

        player.PrintToChat(fullMessage.ToString());
    }

    public void PrintToAll(string message, MsgType type = MsgType.Normal)
    {
        var fullMessage = new StringBuilder(_pluginPrefix)
            .Append($" {messageColors.GetValueOrDefault(type, ChatColors.Default)}")
            .Append(message)
            .Append($"{ChatColors.Default}");

        Server.PrintToChatAll(fullMessage.ToString());
    }
}

public enum MsgType
{
    Normal,
    Warning,
    Error
}