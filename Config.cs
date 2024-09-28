using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace TeamEnforcer;

public class TeamEnforcerConfig : BasePluginConfig
{
    [JsonPropertyName("ChatMessagePrefix")] public string ChatMessagePrefix { get; set; } = $" {ChatColors.DarkBlue}[{ChatColors.LightBlue}TeamEnforcer{ChatColors.DarkBlue}]{ChatColors.Default}";
    [JsonPropertyName("RoundsInCtToLowPrio")] public int RoundsInCtToLowPrio { get; set; } = 2;
}