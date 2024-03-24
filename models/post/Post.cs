using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Post
{
    [JsonPropertyName("timestamp")]
    public long Timestamp_s { get; set; }

    [JsonPropertyName("attachments")]
    public List<PostMediaData> PostAttachments { get; set; }

    [JsonPropertyName("data")]
    public List<PostData> PostData { get; set; }
}

public class PostMediaData
{
    [JsonPropertyName("data")]
    public List<PostMediaDataMedia> PostMediaAttachments { get; set; }
}

public class PostMediaDataMedia
{
    [JsonPropertyName("media")]
    public PostMediaAttachment PostMediaAttachments { get; set; }
}

public class PostData
{
    [JsonPropertyName("update_timestamp")]
    public long UpdateTimestamp { get; set; }

    [JsonPropertyName("post")]
    public string Text { get; set; }
}