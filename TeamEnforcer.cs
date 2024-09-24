using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using TeamEnforcer.Managers;
using TeamEnforcer.Services;

namespace TeamEnforcer;

public partial class TeamEnforcer : BasePlugin
{
    public override string ModuleName => "TeamEnforcer";
    public override string ModuleVersion => "v0.0.1";

    private MessageService? _messageService;
    private QueueManager? _queueManager;
    private TeamManager? _teamManager;

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);
        
        Server.ExecuteCommand("mp_autoteambalance false");

        _messageService = new($" {ChatColors.DarkBlue}[{ChatColors.LightBlue}TeamEnforcer{ChatColors.DarkBlue}]{ChatColors.Default}");
        _queueManager = new(_messageService, this);
        _teamManager = new(_queueManager, _messageService, this);
    }

    public override void Unload(bool hotReload)
    {
        base.Unload(hotReload);
    }
}
