
using System.Text.Json.Serialization;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class BlockImageFormat
    {
        [JsonPropertyName("block_width")]
        public int BlockWidth { get; set; }

        [JsonPropertyName("block_height")]
        public int BlockHeight { get; set; }

        [JsonPropertyName("display_source")]
        public string DisplaySource { get; set; }

        [JsonPropertyName("block_full_width")]
        public bool BlockFullWidth { get; set; }

        [JsonPropertyName("block_page_width")]
        public bool BlockPageWidth { get; set; }

        [JsonPropertyName("block_aspect_ratio")]
        public float BlockAspectRatio { get; set; }

        [JsonPropertyName("block_preserve_scale")]
        public bool BlockPreserveScale { get; set; }
    }
}
