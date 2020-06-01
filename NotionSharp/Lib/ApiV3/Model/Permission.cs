using System;
using Newtonsoft.Json;
using NotionSharp.Lib.ApiV3.Enums;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class Permission
    {
        public const string TypeUser = "user_permission";
        public const string TypePublic = "public_permission";

        /// <summary>
        /// editor, reader
        /// </summary>
        public Role Role { get; set; }

        /// <summary>
        /// user_permission, public_permission
        /// </summary>
        public string Type { get; set; }

        [JsonProperty("user_id")]
        public Guid UserId { get; set; }
        [JsonProperty("allow_duplicate")]
        public bool AllowDuplicate { get; set; }
        [JsonProperty("allow_search_engine_indexing")]
        public bool AllowSearchEngineIndexing { get; set; }
    }
}

