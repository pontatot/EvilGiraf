using System.Text.Json.Serialization;

namespace EvilGiraf.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ApplicationType
{
    Docker,
    Git
}