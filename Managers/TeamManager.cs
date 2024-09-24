using System.Runtime.Versioning;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using TeamEnforcer.Helpers;
using TeamEnforcer.Services;

namespace TeamEnforcer.Managers;

public class TeamManager(QueueManager queueManager, MessageService messageService, TeamEnforcer plugin)
{
    public readonly MessageService _messageService = messageService;
    public readonly TeamEnforcer _plugin = plugin;

    private readonly Random _random = new();
    private readonly QueueManager _queueManager = queueManager;
    private readonly HashSet<CCSPlayerController> _noCtList = [];
    
    private readonly double ctRatio = 0.25;

    private readonly Stack<CCSPlayerController> ctJoinOrder = [];
    private readonly HashSet<CCSPlayerController> legitCtJoins = [];
    private readonly List<CCSPlayerController> _leaveCtList = [];

    public void AddToLeaveList(CCSPlayerController? player)
    {
        if (player == null || !player.IsReal()) return;

        if (player.Team != CsTeam.CounterTerrorist) return;

        if (_leaveCtList.Contains(player)) return;

        _leaveCtList.Add(player);
        _messageService.PrintMessage(player, _plugin.Localizer["TeamEnforcer.AddedToLeaveList"]);
    }
    
    public void BalanceTeams()
    {
        foreach (var leaver in _leaveCtList)
        {
            if (leaver == null || !leaver.IsReal()) continue;
            _leaveCtList.Remove(leaver);
            
            DemoteToT(leaver);
            _messageService.PrintMessage(leaver, _plugin.Localizer["TeamEnforcer.DemotedFromLeaversList", "!t"]);
        }

        int playersPromoted = 0;
        
        List<CCSPlayerController> players = Utilities.GetPlayers();

        int totalCtandT = players.FindAll(p => p.Team == CsTeam.Terrorist || p.Team == CsTeam.CounterTerrorist).Count;
        int ctCount = players.FindAll(p => p.Team == CsTeam.CounterTerrorist).Count;

        int idealCtCount = (int) (totalCtandT * ctRatio);
        
        if (ctCount < idealCtCount)
        {
            var promotionsNeeded = idealCtCount - ctCount;
            List<CCSPlayerController> promotionList = _queueManager.GetNextInQueue(promotionsNeeded);

            foreach (var player in promotionList)
            {
                PromoteToCt(player);
                _messageService.PrintToAll(_plugin.Localizer["TeamEnforcer.TPromotedFromQueue", player.PlayerName ?? "<John Doe>"]);
                playersPromoted++;
            }

            if (promotionList.Count < promotionsNeeded)
            {
                var randomsNeeded = promotionsNeeded - promotionList.Count;
                List<CCSPlayerController> randomsList = GetRandoms(randomsNeeded, CsTeam.Terrorist);
                foreach (var player in randomsList)
                {
                    PromoteToCt(player);
                    _messageService.PrintToAll(_plugin.Localizer["TeamEnforcer.RandomTPromoted", player.PlayerName ?? "<John Doe>"]);
                    playersPromoted++;
                }
            }

            if (playersPromoted < promotionsNeeded)
            {
                _messageService.PrintToAll(_plugin.Localizer["TeamEnforcer.NotEnoughTsAvailable"]);
            }
        }
        else if (ctCount > idealCtCount)
        {
            int demotionsNeeded = idealCtCount - ctCount;

            int demotedCount = 0;
            var illegitimateCts = Utilities.GetPlayers().FindAll(p => p.Team == CsTeam.CounterTerrorist && !legitCtJoins.Contains(p));

            if (illegitimateCts.Count >= demotionsNeeded)
            {
                var illegitimateCtsToDemote = illegitimateCts.Take(demotionsNeeded);
                foreach (var ct in illegitimateCts)
                {
                    if (ct == null || !ct.IsReal()) continue;

                    DemoteToT(ct);
                    _messageService.PrintMessage(ct, _plugin.Localizer["TeamEnforcer.DemotedJoinedIllegitimately"]);
                    demotedCount++;
                }
            }

            while (ctJoinOrder.Count >= 0 && demotedCount < demotionsNeeded)
            {
                var nextCt = ctJoinOrder.Pop();

                if (nextCt == null || !nextCt.IsReal() || nextCt.Team != CsTeam.CounterTerrorist) continue;

                DemoteToT(nextCt);
                _messageService.PrintMessage(nextCt, _plugin.Localizer["TeamEnforcer.DemotedFromStack"]);
                demotedCount++;
            }
        }
    }

    public void DemoteToT(CCSPlayerController? player)
    {
        if (player == null || !player.IsReal()) return;

        if (player.Team == CsTeam.Terrorist) return;

        legitCtJoins.Remove(player);
        player.SwitchTeam(CsTeam.Terrorist);
        player.CommitSuicide(false, true);
    }

    public void PromoteToCt(CCSPlayerController? player)
    {
        if (player == null || !player.IsReal()) return;

        if (player.Team == CsTeam.CounterTerrorist) return;

        ctJoinOrder.Push(player);
        legitCtJoins.Add(player);
        player.SwitchTeam(CsTeam.CounterTerrorist);
        player.CommitSuicide(false, true);
    }

    public List<CCSPlayerController> GetRandoms(int count, CsTeam team)
    {
        List<CCSPlayerController> randomsList = new(count);

        if (randomsList.Count < count)
        {
            var teamsPlayers = Utilities.GetPlayers()
                .FindAll(p => p != null && p.IsReal() && p.Team == team);

            while(randomsList.Count < count && teamsPlayers.Count > 0)
            {
                var randomIndex = _random.Next(teamsPlayers.Count);
                var randomPlayer = teamsPlayers[randomIndex];

                if (randomPlayer != null && randomPlayer.IsReal())
                {
                    randomsList.Add(randomPlayer);
                    teamsPlayers.RemoveAt(randomIndex);
                }
            }
        }

        return randomsList;
    }
    
    public void JoinNoCtList(CCSPlayerController? player)
    {
        if (player == null || !player.IsReal()) return;

        if (player.Team != CsTeam.Terrorist) return;

        if (_noCtList.Contains(player)) return;

        _noCtList.Add(player);
    }
}