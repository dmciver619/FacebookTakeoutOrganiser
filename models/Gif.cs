using System.Text.Json.Serialization;

public class Gif
{
    [JsonPropertyName("uri")]
    public string URI { get; set; }
}