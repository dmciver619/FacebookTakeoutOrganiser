using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Archive
{
    [JsonPropertyName("archive_v2")]
    public List<Post> ArchivedPosts { get; set; }
}