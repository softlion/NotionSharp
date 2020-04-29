using System;
using Newtonsoft.Json;
using NotionSharp.Lib.ApiV3.Enums;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class Permission
    {
        public Role Role { get; set; }
        /// <summary>
        /// user_permission, public_permission
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// user_permission
        /// </summary>
        [JsonProperty("user_id")]
        public Guid UserId { get; set; }

        /// <summary>
        /// public_permission
        /// </summary>
        [JsonProperty("allow_duplicate")]
        public bool AllowDuplicate { get; set; }
        /// <summary>
        /// public_permission
        /// </summary>
        [JsonProperty("allow_search_engine_indexing")]
        public bool AllowSearchEngineIndexing { get; set; }
    }
}
