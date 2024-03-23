using System.Text.Json.Serialization;

public class Video : ILocatedFile
{
    [JsonPropertyName("uri")]
    public string URI { get; set; }

    [JsonPropertyName("creation_timestamp")]
    public long CreateTimestamp { get; set; }
}