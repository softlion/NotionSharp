using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NotionSharp.ApiClient
{
    public class PaginationResult<T> : BaseObject
    {
        public List<T>? Results { get; set; }
        [JsonPropertyName("next_cursor")]
        public string? NextCursor { get; set; }
        [JsonPropertyName("has_more")]
        public bool HasMore { get; set; }
    }
}