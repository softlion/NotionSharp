namespace NotionSharp.ApiClient;

public record SearchRequest
{
    public string? Query { get; init; }
    public SortOptions? Sort { get; init; }
    public FilterOptions? Filter { get; init; }
    public string? StartCursor { get; init; }
    public int PageSize { get; init; }
}