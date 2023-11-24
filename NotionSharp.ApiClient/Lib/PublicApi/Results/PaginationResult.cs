using System.Collections.Generic;

namespace NotionSharp.ApiClient;

public class PaginationResult<T> : NamedObject
{
    public List<T>? Results { get; init; }
    public string? NextCursor { get; init; }
    public bool HasMore { get; init; }
}