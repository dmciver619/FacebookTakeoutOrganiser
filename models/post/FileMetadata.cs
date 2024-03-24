using System.Collections.Generic;
using System.Text.Json.Serialization;

public class FileMetadata
{
    [JsonPropertyName("exif_data")]
    public List<Metadata> MetadataEntries { get; set; }
}