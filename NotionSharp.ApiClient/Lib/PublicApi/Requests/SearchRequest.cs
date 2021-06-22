using System.Text.Json.Serialization;

namespace NotionSharp.ApiClient
{
    public struct SearchRequest
    {
        public string? Query { get; set; }
        public SortOptions? Sort { get; set; }
        public FilterOptions? Filter { get; set; }
        [JsonPropertyName("start_cursor")]
        public string? StartCursor { get; set; }
        [JsonPropertyName("page_size")]
        public int PageSize { get; set; }
    }
}