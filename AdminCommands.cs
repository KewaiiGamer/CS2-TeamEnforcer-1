using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using TeamEnforcer.Helpers;

namespace TeamEnforcer;

public partial class TeamEnforcer
{
    [ConsoleCommand("css_legitjoins")]
    public void OnLegitJoinCommand(CCSPlayerController? invoker, CommandInfo commandInfo)
    {
        if (invoker == null || !invoker.IsReal()) return;
        
        var msg = _teamManager?.GetLegitCtsString();

        commandInfo.ReplyToCommand(msg ?? "Unable to find list.");
    }
}