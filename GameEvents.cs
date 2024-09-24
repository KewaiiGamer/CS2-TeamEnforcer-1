using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;

namespace TeamEnforcer;

public partial class TeamEnforcer
{
    [GameEventHandler]
    public HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo eventInfo)
    {
        _teamManager?.BalanceTeams();
        return HookResult.Continue;
    }
}
