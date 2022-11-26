using System;
using System.Text.Json.Serialization;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class CursorStack
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }
        [JsonPropertyName("index")]
        public int Index { get; set; }
        [JsonPropertyName("table")]
        public string Table { get; set; }
    }
}
