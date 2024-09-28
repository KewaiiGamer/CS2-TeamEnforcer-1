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
    
    public readonly string PluginPrefix = pluginPrefix;

    public void PrintToConsole(string message)
    {
        var fullMessage = new StringBuilder("[TeamEnforcer] ").Append(message);
        Console.WriteLine(fullMessage.ToString());
    }

    public void PrintMessage(CCSPlayerController? player, string message, MsgType type = MsgType.Normal)
    {
        if (player == null || !player.IsReal()) return;
        
        var fullMessage = new StringBuilder(PluginPrefix)
            .Append($" {messageColors.GetValueOrDefault(type, ChatColors.Default)}")
            .Append(message)
            .Append($"{ChatColors.Default}");

        player.PrintToChat(fullMessage.ToString());
    }

    public string GetMessageString(string message, MsgType type = MsgType.Normal)
    {
        var fullMessage = new StringBuilder(PluginPrefix)
            .Append($" {messageColors.GetValueOrDefault(type, ChatColors.Default)}")
            .Append(message)
            .Append($"{ChatColors.Default}");

        return fullMessage.ToString();
    }

    public void PrintToAll(string message, MsgType type = MsgType.Normal)
    {
        var fullMessage = new StringBuilder(PluginPrefix)
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