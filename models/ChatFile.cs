using System.Collections.Generic;
using System.Text.Json.Serialization;

public class ChatFile
{
    [JsonPropertyName("title")]
    public string ChatName { get; set; }

    [JsonPropertyName("is_still_participant")]
    public bool StillParticipant { get; set; }

    [JsonPropertyName("thread_path")]
    public string RelativePath { get; set; }

    [JsonPropertyName("magic_words")]
    public List<string> MagicWords { get; set; }

    [JsonPropertyName("joinable_mode")]
    public JoinableMode JoinableMode { get; set; }

    [JsonPropertyName("participants")]
    public List<dynamic> Participants { get; set; }

    [JsonPropertyName("messages")]
    public List<ChatEntry> Messages { get; set; }
    
    [JsonPropertyName("image")]
    public Photo Image { get; set; }
}