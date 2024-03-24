using System.Text.Json.Serialization;

public class PostMediaAttachment : ILocatedFile
{
    [JsonPropertyName("uri")]
    public string URI { get; set; }

    [JsonPropertyName("creation_timestamp")]
    public long CreateTimestamp { get; set; }
    
    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("media_metadata")]
    public PostMediaMetadata Metadata { get; set; }
}