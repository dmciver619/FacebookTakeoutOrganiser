using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Videos
{
    [JsonPropertyName("videos_v2")]
    public List<PostMediaAttachment> ArchivedPosts { get; set; }
}