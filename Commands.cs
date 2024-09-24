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

        return;
    }

    [ConsoleCommand("css_t")]
    public void OnTCommand(CCSPlayerController? invoker, CommandInfo commandInfo)
    {
        if (invoker == null || !invoker.IsReal() || invoker.Team != CsTeam.CounterTerrorist) return;

        _teamManager?.AddToLeaveList(invoker);

        return;
    }

    [ConsoleCommand("css_noct")]
    public void OnNoctCommand(CCSPlayerController? invoker, CommandInfo commandInfo)
    {
        // Prevent from being added to queue
        // Remove from queue if they are in it
        return;
    }

    [ConsoleCommand("css_lq")]
    [ConsoleCommand("css_leavequeue")]
    public void OnLeaveQueueCommand(CCSPlayerController? invoker, CommandInfo commandInfo)
    {
        // Check if in a queue
        // Remove from queue
        return;
    }

    [ConsoleCommand("css_vq")]
    [ConsoleCommand("css_queue")]
    [ConsoleCommand("css_viewqueue")]
    public void OnViewQueueCommand(CCSPlayerController? invoker, CommandInfo commandInfo)
    {
        // Check if queue is empty
        // Print queue
        // Print position if invoker in queue
        return;
    }

    
}