using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using TeamEnforcer.Helpers;
using CounterStrikeSharp.API.Modules.Utils;
using TeamEnforcer.Services;

namespace TeamEnforcer;

public partial class TeamEnforcer
{
    public void RegisterListeners()
    {
        AddCommandListener("jointeam", OnJoinTeamCommand);

        if (_teamManager != null) RegisterListener<Listeners.OnMapEnd>(_teamManager.PrepareForNewMap);

        _messageService?.PrintToConsole("Registered Listeners");
    }

    public HookResult OnJoinTeamCommand(CCSPlayerController? invoker, CommandInfo commandInfo)
    {
        if (invoker == null || !invoker.IsReal()) return HookResult.Continue;

        if (commandInfo.ArgByIndex(1) == "3") // Trying to join CT
        {
            _messageService?.PrintMessage(
                invoker,
                Localizer["TeamEnforcer.CannotJoinCt", $"{ChatColors.Blue}!guard{ChatColors.Default}"],
                MsgType.Error
            );
            return HookResult.Handled;
        }

        if (commandInfo.ArgByIndex(1) == "2" && invoker.Team == CsTeam.CounterTerrorist) // Trying to join T from CT
        {
            _messageService?.PrintMessage(
                invoker,
                Localizer["TeamEnforcer.CannotLeaveCt", $"{ChatColors.Blue}!t{ChatColors.Default}"]
            );
            _teamManager?.AddToLeaveList(invoker);
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }
}