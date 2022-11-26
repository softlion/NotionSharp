using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class RecordMap
    {
        [JsonPropertyName("notion_user")]
        public Dictionary<Guid, NotionUser> NotionUser { get; set; }

        [JsonPropertyName("user_root")]
        public Dictionary<Guid, UserRoot> UserRoot { get; set; }

        [JsonPropertyName("user_settings")]
        public Dictionary<Guid, UserSettings> UserSettings { get; set; }

        [JsonPropertyName("space_view")]
        public Dictionary<Guid, SpaceView> SpaceView { get; set; }
        public Dictionary<Guid, Space> Space { get; set; }

        public Dictionary<Guid, Block> Block { get; set; }

        public Dictionary<Guid, Collection> Collection { get; set; }
    }
}
