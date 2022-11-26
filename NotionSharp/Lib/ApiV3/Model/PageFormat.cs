using System;
using System.Text.Json.Serialization;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class PageFormat
    {
        /// <summary>
        /// Url or string of the icon
        /// </summary>
        [JsonPropertyName("page_icon")]
        public string PageIcon { get; set; }

        [JsonPropertyName("page_cover_position")]
        public double PageCoverPositon { get; set; }

        /// <summary>
        /// type=callout
        /// </summary>
        [JsonPropertyName("block_color")]
        public string BlockColor { get; set; }

        [JsonPropertyName("block_locked")]
        public bool BlockLocked { get; set; }

        [JsonPropertyName("block_locked_by")]
        public Guid BlockLockedBy { get; set; }
    }
}
