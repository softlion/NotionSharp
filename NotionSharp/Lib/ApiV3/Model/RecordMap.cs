using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class RecordMap
    {
        [JsonProperty("notion_user")]
        public Dictionary<Guid, NotionUser> NotionUser { get; set; }

        [JsonProperty("user_root")]
        public Dictionary<Guid, UserRoot> UserRoot { get; set; }

        [JsonProperty("user_settings")]
        public Dictionary<Guid, UserSettings> UserSettings { get; set; }

        [JsonProperty("space_view")]
        public Dictionary<Guid, SpaceView> SpaceView { get; set; }
        public Dictionary<Guid, Space> Space { get; set; }

        public Dictionary<Guid, Block> Block { get; set; }

        public Dictionary<Guid, Collection> Collection { get; set; }
    }
}
