using System;
using System.Text.Json.Serialization;
using NotionSharp.Lib.ApiV3.Model;

namespace NotionSharp.Lib.ApiV3.Requests;

public class LoadPageChunkRequest
{
    [JsonPropertyName("chunkNumber")]
    public int ChunkNumber { get; set; }
    [JsonPropertyName("cursor")]
    public Cursor Cursor { get; set; }
    [JsonPropertyName("limit")]
    public int Limit { get; set; }
    [JsonPropertyName("pageId")]
    public Guid PageId { get; set; }
    [JsonPropertyName("verticalColumns")]
    public bool VerticalColumns { get; set; }
}

public class PageSpecification
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    [JsonPropertyName("spaceId")]
    public Guid SpaceId { get; set; }
}

public class LoadCachedPageChunkRequest
{
    [JsonPropertyName("page")]
    public PageSpecification Page { get; set; }
    
    [JsonPropertyName("limit")]
    public int Limit { get; set; }

    [JsonPropertyName("cursor")]
    public Cursor Cursor { get; set; }

    [JsonPropertyName("chunkNumber")]
    public int ChunkNumber { get; set; }

    [JsonPropertyName("verticalColumns")]
    public bool VerticalColumns { get; set; }
}
