namespace NotionSharp.ApiClient;

public class Parent
{
    /// <summary>
    /// database_id, block_id, workspace, page_id
    /// </summary>
    public string Type { get; init; }
        
    public string? DatabaseId { get; init; }
    public string? BlockId { get; init; }
    public string? Workspace { get; init; }
    public string? PageId { get; init; }
}