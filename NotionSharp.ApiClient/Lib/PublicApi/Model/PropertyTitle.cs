using System.Collections.Generic;

namespace NotionSharp.ApiClient;

public record PropertyTitle
{
    public string Type { get; init; } = "title";
    public string Id { get; init; } //This is not a block id
    public List<RichText> Title { get; init; }
}