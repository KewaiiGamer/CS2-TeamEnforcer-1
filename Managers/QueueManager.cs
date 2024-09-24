using System.Text;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using TeamEnforcer.Collections;
using TeamEnforcer.Helpers;
using TeamEnforcer.Services;

namespace TeamEnforcer.Managers;

public class QueueManager(MessageService messageService, TeamEnforcer plugin)
{
    public readonly MessageService _messageService = messageService;
    public readonly TeamEnforcer _plugin = plugin;

    private readonly CustomQueue<CCSPlayerController> _priorityQueue = new("Priority Queue");
    private readonly CustomQueue<CCSPlayerController> _mainQueue = new("Main Queue");
    private readonly CustomQueue<CCSPlayerController> _lowPriorityQueue = new("Low Priority Queue");


    public void JoinQueue(CCSPlayerController? player, QueuePriority prio = QueuePriority.Normal)
    {
        if (player == null || !player.IsReal()) return;

        if (_priorityQueue.Contains(player) || _mainQueue.Contains(player) || _lowPriorityQueue.Contains(player)) return;

        switch(prio)
        {
            case QueuePriority.High:
                _priorityQueue.Enqueue(player);
                _messageService.PrintMessage(player, _plugin.Localizer["TeamEnforcer.AddedToPriorityQueue"]);
                break;
            case QueuePriority.Low:
                _lowPriorityQueue.Enqueue(player);
                _messageService.PrintMessage(player, _plugin.Localizer["TeamEnforcer.JoinedQueue"]);
                break;
            default:
                _mainQueue.Enqueue(player);
                _messageService.PrintMessage(player, _plugin.Localizer["TeamEnforcer.JoinedQueue"]);
                break;
        }
    }

    public void LeaveQueue(CCSPlayerController? player)
    {
        if (player == null || !player.IsReal()) return;

        if (!_priorityQueue.Contains(player) && !_mainQueue.Contains(player) && !_lowPriorityQueue.Contains(player)) return;

        var queues = new List<CustomQueue<CCSPlayerController>>{_priorityQueue, _mainQueue, _lowPriorityQueue};

        foreach (var queue in queues)
        {
            if (queue.Contains(player))
            {
                queue.Remove(player);
            }
        }

        _messageService.PrintMessage(player, _plugin.Localizer["TeamEnforcer.RemovedFromQueue"]);
    }

    public bool IsPlayerInQueue(CCSPlayerController? player, out PlayerQueueStatus? status)
    {
        status = new PlayerQueueStatus("None", -1);

        if (player == null || !player.IsReal()) return false;

        Dictionary<string, CustomQueue<CCSPlayerController>> queues = new(){
            {"Priority Queue", _priorityQueue},
            {"Main Queue", _mainQueue},
            {"Low Priority Queue", _lowPriorityQueue}, 
        };

        foreach (var queue in queues)
        {
            if (queue.Value.Contains(player))
            {
                status.queueName = queue.Key;
                status.queuePosition = queue.Value.GetQueuePosition(player);
                return true;
            }
        }

        return false;
    }

    public List<CCSPlayerController> GetNextInQueue(int count)
    {
        List<CCSPlayerController> nextList = [];

        var queues = new List<CustomQueue<CCSPlayerController>>{_priorityQueue, _mainQueue, _lowPriorityQueue};

        foreach(var queue in queues)
        {
            while (nextList.Count < count && queue.Count > 0)
            {
                CCSPlayerController? player = queue.Dequeue();
                if (player == null || !player.IsReal() || player.Team == CsTeam.CounterTerrorist) continue;

                nextList.Add(player);
            }

            if (nextList.Count >= count) break;
        }

        return nextList;
    }
    public bool IsQueueEmpty()
    {
        if (_lowPriorityQueue.Count == 0 && _priorityQueue.Count == 0 && _mainQueue.Count == 0) return true;
        return false;
    }
    public string GetQueueStatus()
    {
        // Assumes queue isnt empty, command checks that
        var statusMessage = new StringBuilder($" Queue status:\u2029");
        int count = 1;
        if (_mainQueue.Count == 0 && _lowPriorityQueue.Count == 0 && _priorityQueue.Count == 0) return "";

        if (_priorityQueue.Count > 0)
        {
            foreach (var player in _priorityQueue.GetAllItems())
            {
                if (player == null || !player.IsReal()) continue;
                statusMessage.Append($"#{count} - {player.PlayerName ?? "<John Doe>"}\u2029");
                count++;
            }

            foreach (var player in _mainQueue.GetAllItems())
            {
                if (player == null || !player.IsReal()) continue;
                statusMessage.Append($"#{count} - {player.PlayerName ?? "<John Doe>"}\u2029");
                count++;
            }

            foreach (var player in _lowPriorityQueue.GetAllItems())
            {
                if (player == null || !player.IsReal()) continue;
                statusMessage.Append($"#{count} - {player.PlayerName ?? "<John Doe>"}\u2029");
                count++;
            }
        }

        return statusMessage.ToString();
    }
}

public enum QueuePriority
{
    High,
    Normal,
    Low
}

public class PlayerQueueStatus(string name, int pos)
{
    public string queueName = name;
    public int queuePosition = pos;
}