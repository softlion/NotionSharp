using System;
using System.Text.Json.Serialization;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class Permission
    {
        public const string RoleEditor = "editor";
        public const string RoleReader = "reader";
        public const string RoleCommentOnly = "comment_only";

        public const string TypeUser = "user_permission";
        public const string TypePublic = "public_permission";

        /// <summary>
        /// editor, reader
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// user_permission, public_permission
        /// </summary>
        public string Type { get; set; }

        [JsonPropertyName("user_id")]
        public Guid UserId { get; set; }
        [JsonPropertyName("allow_duplicate")]
        public bool AllowDuplicate { get; set; }
        [JsonPropertyName("allow_search_engine_indexing")]
        public bool AllowSearchEngineIndexing { get; set; }
    }
}

