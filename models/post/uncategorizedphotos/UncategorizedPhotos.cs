using System.Collections.Generic;
using System.Text.Json.Serialization;

public class UncategorizedPhotos
{
    [JsonPropertyName("other_photos_v2")]
    public List<PostMediaAttachment> OtherPhotos { get; set; }
}