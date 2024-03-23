using System.Collections.Generic;
using System.Text.Json.Serialization;

public class ChatEntry
{
    [JsonPropertyName("sender_name")]
    public string Sender_Name { get; set; }

    [JsonPropertyName("timestamp_ms")]
    public long Timetamp_ms { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("reactions")]
    public List<Reaction> Reactions { get; set; }

    [JsonPropertyName("gifs")]
    public List<Gif> Gifs { get; set; }

    [JsonPropertyName("photos")]
    public List<Photo> Photos { get; set; }

    [JsonPropertyName("videos")]
    public List<Video> Videos { get; set; }
    
    [JsonPropertyName("stickers")]
    public List<Sticker> Stickers { get; set; }
}