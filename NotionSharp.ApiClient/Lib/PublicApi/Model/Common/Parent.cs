using System.Text.Json.Serialization;

namespace NotionSharp.ApiClient;

public class Parent
{
    /// <summary>
    /// database_id, block_id, workspace, page_id
    /// </summary>
    public string Type { get; init; }
        
    [JsonPropertyName("database_id")]
    public string? DatabaseId { get; init; }
    [JsonPropertyName("block_id")]
    public string? BlockId { get; init; }
    public string? Workspace { get; init; }
    [JsonPropertyName("page_id")]
    public string? PageId { get; init; }
}