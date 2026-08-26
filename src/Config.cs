using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Hideout;

namespace Leaves_ProgressiveContainer;

// Mirrors config/config.jsonc. Requirements are deserialized straight into SPT's own
// hideout Requirement model since the field names already line up (templateId, count, areaType, requiredLevel, type).
public record ModConfig
{
    [JsonPropertyName("removeContainersFromPeacekeeper")]
    public required bool RemoveContainersFromPeacekeeper { get; set; }

    [JsonPropertyName("craftingTime")]
    public required int CraftingTime { get; set; }

    [JsonPropertyName("secure_containers")]
    public required Dictionary<string, ContainerConfig> SecureContainers { get; set; }
}

public record ContainerConfig
{
    [JsonPropertyName("requirements")]
    public required List<Requirement> Requirements { get; set; }
}
