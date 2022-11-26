using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using NotionSharp.Lib.ApiV3.Enums;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class Block : BaseModel
    {
        /// <summary>
        /// page, collection_view_page
        /// text, image, sub_header, sub_sub_header, quote, code, bookmark
        /// </summary>
        public string Type => Value.GetProperty("type").GetString();
        public JsonElement? Properties => Value.TryGetProperty("properties", out var value) ? value : (JsonElement?)null;

        #region type=page
        public string? Title => Properties?.TryGetProperty("title", out var title) == true ? title[0][0].GetString() : null;
        public List<Guid>? Content => Value.TryGetProperty("content", out var content) ? content.EnumerateArray().Select(v => v.GetGuid()).ToList() : null;
        #endregion

        #region type=collection_view_page
        public List<Guid>? ViewIds => Value.TryGetProperty("view_ids", out var viewIds) ? viewIds.EnumerateArray().Select(v => v.GetGuid()).ToList() : null;
        public Guid CollectionId => Value.GetProperty("collection_id").GetGuid();
        #endregion

        public PageFormat? Format => Value.TryGetProperty("format", out var format) ? format.Deserialize<PageFormat>() : null;
        public ColumnFormat? ColumnFormat => Value.TryGetProperty("format", out var format) ? format.Deserialize<ColumnFormat>() : null;

        public List<Permission>? Permissions => Value.TryGetProperty("permissions", out var permissions) ? permissions.Deserialize<List<Permission>>() : null;
        public List<string>? EmailDomains => Value.TryGetProperty("email_domains", out var emailDomains) ? emailDomains.Deserialize<List<string>>() : null;

        public long CreatedTime => Value.GetProperty("created_time").GetInt64();
        public long LastEditedTime => Value.GetProperty("last_edited_time").GetInt64();
        public Guid CreatedBy => Value.GetProperty("created_by").GetGuid();
        public Guid LastEditedBy => Value.GetProperty("last_edited_by").GetGuid();
        public Guid ParentId => Value.GetProperty("parent_id").GetGuid();
        public bool Alive => Value.GetProperty("alive").GetBoolean();
        public Guid CreatedById => Value.GetProperty("created_by_id").GetGuid();
        public Guid LastEditedById => Value.GetProperty("last_edited_by_id").GetGuid();
        /// <summary>
        /// collection_view_page only
        /// </summary>
        public Guid CopiedFrom => Value.GetProperty("copied_from").GetGuid();
        public ParentTable ParentTable => Value.TryGetProperty("parent_table", out var parentTable) ? parentTable.Deserialize<ParentTable>() : ParentTable.Unknown;
        // "created_by_table": "notion_user",
        // "last_edited_by_table": "notion_user",
    }
}
