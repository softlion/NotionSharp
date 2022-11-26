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
        public string Type => TheValue.GetProperty("type").GetString();
        public JsonElement? Properties => TheValue.TryGetProperty("properties", out var value) ? value : (JsonElement?)null;

        #region type=page
        public string? Title => Properties?.TryGetProperty("title", out var title) == true ? title[0][0].GetString() : null;
        public List<Guid>? Content => TheValue.TryGetProperty("content", out var content) ? content.EnumerateArray().Select(v => v.GetGuid()).ToList() : null;
        #endregion

        #region type=collection_view_page
        public List<Guid>? ViewIds => TheValue.TryGetProperty("view_ids", out var viewIds) ? viewIds.EnumerateArray().Select(v => v.GetGuid()).ToList() : null;
        public Guid CollectionId => TheValue.GetProperty("collection_id").GetGuid();
        #endregion

        public PageFormat? Format => TheValue.TryGetProperty("format", out var format) ? format.Deserialize<PageFormat>() : null;
        public ColumnFormat? ColumnFormat => TheValue.TryGetProperty("format", out var format) ? format.Deserialize<ColumnFormat>() : null;

        public List<Permission>? Permissions => TheValue.TryGetProperty("permissions", out var permissions) ? permissions.Deserialize<List<Permission>>() : null;
        public List<string>? EmailDomains => TheValue.TryGetProperty("email_domains", out var emailDomains) ? emailDomains.Deserialize<List<string>>() : null;

        public long CreatedTime => TheValue.GetProperty("created_time").GetInt64();
        public long LastEditedTime => TheValue.GetProperty("last_edited_time").GetInt64();
        public Guid CreatedBy => TheValue.GetProperty("created_by").GetGuid();
        public Guid LastEditedBy => TheValue.GetProperty("last_edited_by").GetGuid();
        public Guid ParentId => TheValue.GetProperty("parent_id").GetGuid();
        public bool Alive => TheValue.GetProperty("alive").GetBoolean();
        public Guid CreatedById => TheValue.GetProperty("created_by_id").GetGuid();
        public Guid LastEditedById => TheValue.GetProperty("last_edited_by_id").GetGuid();
        /// <summary>
        /// collection_view_page only
        /// </summary>
        public Guid CopiedFrom => TheValue.GetProperty("copied_from").GetGuid();
        public ParentTable ParentTable => TheValue.TryGetProperty("parent_table", out var parentTable) ? parentTable.Deserialize<ParentTable>() : ParentTable.Unknown;
        // "created_by_table": "notion_user",
        // "last_edited_by_table": "notion_user",
    }
}
