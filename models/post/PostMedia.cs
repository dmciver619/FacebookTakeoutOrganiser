using System.Text.Json.Serialization;

public class PostMedia
{
    [JsonPropertyName("media")]
    public PostMediaAttachment Media { get; set; }
}