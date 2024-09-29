using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using TeamEnforcer.Managers;
using TeamEnforcer.Services;

namespace TeamEnforcer;

public partial class TeamEnforcer : BasePlugin, IPluginConfig<TeamEnforcerConfig>
{
    public override string ModuleName => "TeamEnforcer";
    public override string ModuleVersion => "v0.0.1";

    public TeamEnforcerConfig Config { get; set; } = new();

    private MessageService? _messageService;
    private QueueManager? _queueManager;
    private TeamManager? _teamManager;

    private string DbConnectionString = string.Empty;
    private static Database? Database;
    private static CTBanService? CTBanService;

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

        if (config.RoundsInCtToLowPrio < 0)
        {
            config.RoundsInCtToLowPrio = 0;
        }

        // I took this check and dbstring code from CS2-SimpleAdmin @ https://github.com/daffyyyy/CS2-SimpleAdmin
        if (config.DatabaseHost.Length < 1 || config.DatabaseName.Length < 1 || config.DatabaseUser.Length < 1)
        {
            Logger.LogCritical("[TeamEnforcer] You need to setup Database credentials in config to use CTBan Feature!");
        }
        else
        {
            MySqlConnectionStringBuilder builder = new()
            {
                Server = config.DatabaseHost,
                Database = config.DatabaseName,
                UserID = config.DatabaseUser,
                Password = config.DatabasePassword,
                Port = (uint)config.DatabasePort,
                Pooling = true,
                MinimumPoolSize = 0,
                MaximumPoolSize = 640,
            };

            DbConnectionString = builder.ConnectionString;
            Database = new Database(DbConnectionString, this);
            CTBanService = new CTBanService(Database);
            CTBanService.CreateTables();
        }

        Config = config;

        _messageService = new(Config.ChatMessagePrefix);
        _queueManager = new(_messageService, this);
        _teamManager = new(_queueManager, _messageService, this, CTBanService);
    }

    public override void Unload(bool hotReload)
    {
        base.Unload(hotReload);
    }
}
