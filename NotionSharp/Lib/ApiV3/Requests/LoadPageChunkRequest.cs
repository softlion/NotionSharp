using System;
using Newtonsoft.Json;
using NotionSharp.Lib.ApiV3.Model;

namespace NotionSharp.Lib.ApiV3.Requests
{
    public class LoadPageChunkRequest
    {
        [JsonProperty("chunkNumber")]
        public int ChunkNumber { get; set; }
        [JsonProperty("cursor")]
        public Cursor Cursor { get; set; }
        [JsonProperty("limit")]
        public int Limit { get; set; }
        [JsonProperty("pageId")]
        public Guid PageId { get; set; }
        [JsonProperty("verticalColumns")]
        public bool VerticalColumns { get; set; }
    }
}
