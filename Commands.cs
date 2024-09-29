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
        
        // Check if the player is CT-banned
        bool isCtBanned = false;

        if (CTBanService != null)
        {
            isCtBanned = CTBanService.PlayerIsCTBanned(invoker);
        } 

        if (isCtBanned)
        {   
            // Notify the player that they are CT-banned and cannot join CT
            Server.NextFrame(() => {
                _messageService?.PrintMessage(invoker, Localizer["TeamEnforcer.CtBannedMessage"]);
            });
            return;
        }

        if (invoker.Team != CsTeam.Terrorist)
        {
            Server.NextFrame(() => {
                _messageService?.PrintMessage(invoker, Localizer["TeamEnforcer.CannotJoinQueueFromNotT"]);
            });
            return;
        }

        var ctCount = Utilities.GetPlayers().FindAll(p => p != null && p.IsReal() && p.Team == CsTeam.CounterTerrorist).Count;

        if (ctCount == 0)
        {
            _teamManager?.PromoteToCt(invoker);
            _messageService?.PrintMessage(invoker, Localizer["TeamEnforcer.CtTeamEmptyInstantlyMoved"]);
            return;
        }

        if (_teamManager?.WasCtLastMap(invoker) ?? false)
        {
            _queueManager?.JoinQueue(invoker, Managers.QueuePriority.Low);
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

        if (invoker.Team == CsTeam.CounterTerrorist)
        {
            commandInfo.ReplyToCommand(_messageService?.GetMessageString(Localizer["TeamEnforcer.MustBeT"]) ?? "");
            return;
        }

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
            commandInfo.ReplyToCommand(_messageService?.GetMessageString(Localizer["TeamEnforcer.NotInQueue"]) ?? "");
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
            commandInfo.ReplyToCommand(_messageService?.GetMessageString(Localizer["TeamEnforcer.QueueEmpty", "!guard"]) ?? "");
            return;
        }

        if (_queueManager.IsPlayerInQueue(invoker, out var playerQueueStatus))
        {
            commandInfo.ReplyToCommand(_messageService?.GetMessageString(Localizer["TeamEnforcer.YourPlaceInQueue", playerQueueStatus?.queuePosition ?? -1]) ?? "");
        }

        var queueStatus = _queueManager?.GetQueueStatus();

        commandInfo.ReplyToCommand(_messageService?.GetMessageString(queueStatus ?? "") ?? "");
    }
}