using System.Text.Json.Serialization;

public class Metadata
{
    [JsonPropertyName("upload_ip")]
    public string UploadIp { get; set; }
    
    [JsonPropertyName("taken_timestamp")]
    public long Timestamp_s { get; set; }
}