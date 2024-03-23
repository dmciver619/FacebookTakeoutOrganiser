using System.Text.Json.Serialization;

public class JoinableMode
{
    [JsonPropertyName("title")]
    public int Mode { get; set; }

    [JsonPropertyName("link")]
    public string JoinLink { get; set; }
}