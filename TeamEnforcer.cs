using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
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
    private TeamEnforcerConfig? Config;

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);

        RegisterListeners();

        AddTimer(5.0f, () =>
        {
            string conVarName = "mp_autoteambalance";
            ConVar? cvar = ConVar.Find(conVarName);

            if (cvar == null)
                return;

            cvar.SetValue(false);

            _messageService?.PrintToConsole("Convar 'mp_autoteambalance' has been set to 'false'");
        });

    }

    public void OnConfigParsed(TeamEnforcerConfig config)
    {
        if (config.ChatMessagePrefix == "")
            config.ChatMessagePrefix = $" {ChatColors.DarkBlue}[{ChatColors.LightBlue}TeamEnforcer{ChatColors.DarkBlue}]{ChatColors.Default}";

        Config = config;

        _messageService = new(Config.ChatMessagePrefix);
        _queueManager = new(_messageService, this);
        _teamManager = new(_queueManager, _messageService, this);
    }

    public override void Unload(bool hotReload)
    {
        base.Unload(hotReload);
    }
}
