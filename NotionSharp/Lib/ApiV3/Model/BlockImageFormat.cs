using Newtonsoft.Json;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class BlockImageFormat
    {
        [JsonProperty("block_width")]
        public int BlockWidth { get; set; }

        [JsonProperty("block_height")]
        public int BlockHeight { get; set; }

        [JsonProperty("display_source")]
        public string DisplaySource { get; set; }

        [JsonProperty("block_full_width")]
        public bool BlockFullWidth { get; set; }

        [JsonProperty("block_page_width")]
        public bool BlockPageWidth { get; set; }

        [JsonProperty("block_aspect_ratio")]
        public float BlockAspectRatio { get; set; }

        [JsonProperty("block_preserve_scale")]
        public bool BlockPreserveScale { get; set; }
    }
}
