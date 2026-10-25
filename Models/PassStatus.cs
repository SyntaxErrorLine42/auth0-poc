using System.Text.Json.Serialization;

namespace _1lab.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PassStatus
{
    ACTIVE,
    CANCELLED
}
