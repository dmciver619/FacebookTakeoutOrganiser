using System.Text.Json.Serialization;

public class PostMediaMetadata
{
    [JsonPropertyName("video_metadata")]
    public FileMetadata VideoMetadata { get; set; }
    
    [JsonPropertyName("photo_metadata")]
    public FileMetadata PhotoMetadata { get; set; }
}