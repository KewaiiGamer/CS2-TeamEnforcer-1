using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using TeamEnforcer.Helpers;
using TeamEnforcer.Services;

namespace TeamEnforcer;

public partial class TeamEnforcer
{
    [ConsoleCommand("css_legitjoins")]
    [CommandHelper(minArgs: 0, usage: "", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/generic")]
    public void OnLegitJoinCommand(CCSPlayerController? invoker, CommandInfo commandInfo)
    {
        var msg = _teamManager?.GetLegitCtsString();

        commandInfo.ReplyToCommand(msg ?? "Unable to find list.");
    }

    [ConsoleCommand("css_forcect")]
    [CommandHelper(minArgs: 1, usage: "<player_name>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/kick")]
    public void OnForceCTCommand(CCSPlayerController? invoker, CommandInfo commandInfo)
    {
    }
    
    [ConsoleCommand("css_ctban")]
    [CommandHelper(minArgs: 1, usage: "<player_name/steam_id> [duration_minutes] [reason]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/ban")]
    public void OnCTBanCommand(CCSPlayerController? invoker, CommandInfo commandInfo)
    {
        if (CTBanService == null){
            commandInfo.ReplyToCommand(_messageService?.GetMessageString(Localizer["TeamEnforcer.CTBanUnavailable"]) ?? "[TeamEnforcer] Ctban features unavailable. Notify server administrator.");
            return;
        }

        var targetString = commandInfo.ArgByIndex(1);
        if (targetString == string.Empty)
        {
            commandInfo.ReplyToCommand(
                _messageService?
                .GetMessageString(Localizer["TeamEnforcer.InvalidTarget", "!ctban <player_name/steam_id>"]) 
                ?? "[TeamEnforcer] Invalid target. Usage: !ctban <player_name/steam_id>");
            return;
        }
        
        var targetPlayer = Utils.FindTarget(targetString);
        if (targetPlayer == null)
        {
            commandInfo.ReplyToCommand(
                _messageService?
                .GetMessageString(Localizer["TeamEnforcer.TargetNotFound", targetString]) 
                ?? $"[TeamEnforcer] Unable to find target: {targetString}");
            return;
        }

        var banDurationValid = uint.TryParse(commandInfo.ArgByIndex(2), out var banDurationInt);
        var expirationDate = banDurationValid && banDurationInt > 0
            ? DateTime.Now.AddMinutes(banDurationInt)
            : (DateTime?)null;

        var banReason = commandInfo.ArgByIndex(3) != string.Empty ? commandInfo.ArgByIndex(3) : "No reason provided.";

        Task.Run(() => {
            HandleCtBanAsync(invoker, targetPlayer, commandInfo, banDurationValid, banDurationInt, expirationDate, banReason);
            Server.NextFrame(() => _teamManager?.RemoveCTBannedPlayers());
        });
    }

    [ConsoleCommand("css_ctunban")]
    [CommandHelper(minArgs: 1, usage: "<player_name/steam_id>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/ban")]
    public void OnCTUnbanCommand(CCSPlayerController? invoker, CommandInfo commandInfo)
    {
        if (CTBanService == null){
            commandInfo.ReplyToCommand(_messageService?.GetMessageString(Localizer["TeamEnforcer.CTBanUnavailable"]) ?? "[TeamEnforcer] Ctban features unavailable. Notify server administrator.");
            return;
        }
    }
    
    [ConsoleCommand("css_ctkick")]
    [CommandHelper(minArgs: 1, usage: "<player_name/steam_id>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/kick")]
    public void OnCTKickCommand(CCSPlayerController? invoker, CommandInfo commandInfo)
    {
        if (CTBanService == null){
            commandInfo.ReplyToCommand(_messageService?.GetMessageString(Localizer["TeamEnforcer.CTBanUnavailable"]) ?? "[TeamEnforcer] Ctban features unavailable. Notify server administrator.");
            return;
        }
    }

    public async void HandleCtBanAsync(CCSPlayerController? invoker, CCSPlayerController targetPlayer, CommandInfo commandInfo, bool banDurationValid, uint banDurationInt, DateTime? expirationDate, string banReason)
    {
        bool isAlreadyBanned = false;
        if (CTBanService != null)
        {
            isAlreadyBanned = await CTBanService.PlayerIsCTBannedAsync(targetPlayer);
        }

        if (isAlreadyBanned)
        {
            // Inform the invoker that the player is already banned
            Server.NextFrame(() => {
                    _messageService?
                    .PrintMessage(invoker, Localizer["TeamEnforcer.AlreadyBanned", targetPlayer.PlayerName]);
            });
            return;
        }


        NewCTBan newCTBan = new()
        {
            PlayerSteamId = targetPlayer.SteamID.ToString() ?? "",
            StaffSteamId = invoker?.SteamID.ToString() ?? "Console",
            BanReason = banReason,
            BanDate = DateTime.Now,
            ExpirationDate = expirationDate,
            Active = true
        };

        // Call the async method to ban the player
        if (CTBanService != null)
        {
            await CTBanService.BanPlayerAsync(newCTBan);
        }

        // Notify the command sender that the player has been banned
        Server.NextFrame(() => {
            _messageService?.PrintToAll(Localizer["TeamEnforcer.BanSuccess", targetPlayer.PlayerName, banDurationValid ? banDurationInt.ToString() + " minutes" : "permanent"]);
        });
    }
}