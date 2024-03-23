using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Sticker
{
    [JsonPropertyName("uri")]
    public string URI { get; set; }
    
    [JsonPropertyName("ai_stickers")]
    public List<dynamic> AiStickers { get; set; }
}