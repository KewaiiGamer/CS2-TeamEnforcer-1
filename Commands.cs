using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using TeamEnforcer.Helpers;

namespace TeamEnforcer;

public partial class TeamEnforcer
{
    [ConsoleCommand("css_ct")]
    [ConsoleCommand("css_guard")]
    public void OnGuardCommand(CCSPlayerController? invoker, CommandInfo commandInfo)
    {
        if (invoker == null || !invoker.IsReal()) return;
        if (invoker.Team != CsTeam.Terrorist)
        {
            _messageService?.PrintMessage(invoker, Localizer["TeamEnforcer.CannotJoinQueueFromNotT"]);
            return;
        }
        
        var ctCount = Utilities.GetPlayers().FindAll(p => p != null && p.IsReal() && p.Team == CsTeam.CounterTerrorist).Count;

        if (ctCount == 0)
        {
            _teamManager?.PromoteToCt(invoker);
            _messageService?.PrintMessage(invoker, Localizer["TeamEnforcer.CtTeamEmptyInstantlyMoved"]);
            return;
        }

        _queueManager?.JoinQueue(invoker);
    }

    [ConsoleCommand("css_t")]
    public void OnTCommand(CCSPlayerController? invoker, CommandInfo commandInfo)
    {
        if (invoker == null || !invoker.IsReal() || invoker.Team != CsTeam.CounterTerrorist) return;

        _teamManager?.AddToLeaveList(invoker);
    }

    [ConsoleCommand("css_noct")]
    public void OnNoctCommand(CCSPlayerController? invoker, CommandInfo commandInfo)
    {
        if (invoker == null || !invoker.IsReal()) return;

        _teamManager?.JoinNoCtList(invoker);
        if (_queueManager?.IsPlayerInQueue(invoker, out var _) ?? false)
        {
            _queueManager.LeaveQueue(invoker);
        }
    }

    [ConsoleCommand("css_lq")]
    [ConsoleCommand("css_leavequeue")]
    public void OnLeaveQueueCommand(CCSPlayerController? invoker, CommandInfo commandInfo)
    {
        if (invoker == null || !invoker.IsReal()) return;

        if (!_queueManager?.IsPlayerInQueue(invoker, out var _) ?? true)
        {
            _messageService?.PrintMessage(invoker, Localizer["TeamEnforcer.NotInQueue"]);
            return;
        }

        _queueManager?.LeaveQueue(invoker);
    }

    [ConsoleCommand("css_vq")]
    [ConsoleCommand("css_queue")]
    [ConsoleCommand("css_viewqueue")]
    public void OnViewQueueCommand(CCSPlayerController? invoker, CommandInfo commandInfo)
    {
        // Print position if invoker in queue
        if (_queueManager?.IsQueueEmpty() ?? true)
        {
            _messageService?.PrintMessage(invoker, Localizer["TeamEnforcer.QueueEmpty", "!guard"]);
            return;
        }

        var queueStatus = _queueManager.GetQueueStatus();

        _messageService?.PrintMessage(invoker, queueStatus);

        if (_queueManager.IsPlayerInQueue(invoker, out var playerQueueStatus))
        {
            _messageService?.PrintMessage(invoker, Localizer["TeamEnforcer.YourPlaceInQueue", playerQueueStatus?.queuePosition ?? -1, playerQueueStatus?.queueName ?? "Unknown Queue"]);
        }
    }
}