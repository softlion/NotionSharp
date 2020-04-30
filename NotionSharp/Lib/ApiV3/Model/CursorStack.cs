using System;
using Newtonsoft.Json;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class CursorStack
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("index")]
        public int Index { get; set; }
        [JsonProperty("table")]
        public string Table { get; set; }
    }
}
