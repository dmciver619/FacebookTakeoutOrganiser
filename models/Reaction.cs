using System.Text.Json.Serialization;

public class Reaction
{
    [JsonPropertyName("reaction")]
    public string ReactionUnicode { get; set; }
    
    [JsonPropertyName("actor")]
    public string Actor { get; set; }
}