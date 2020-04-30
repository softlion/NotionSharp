using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using NotionSharp.Lib.ApiV3.Enums;

namespace NotionSharp.Lib.ApiV3.Model
{
    public class Block : BaseModel
    {
        /// <summary>
        /// page, collection_view_page
        /// </summary>
        public string Type => (string)Value["type"];
        public JObject Properties => Value.ContainsKey("properties") ? (JObject)Value["properties"] : null;

        #region type=page
        public string Title => Properties?.ContainsKey("title") == true ? Properties["title"].ToObject<List<List<string>>>()[0][0] : null;
        public List<Guid> Content => Value.ContainsKey("content") ? Value["content"].ToObject<List<Guid>>() : null;
        #endregion

        #region type=collection_view_page
        public List<Guid> ViewIds => Value.ContainsKey("view_ids") ? Value["view_ids"].ToObject<List<Guid>>() : null;
        public Guid CollectionId => (Guid)Value["collection_id"];

        #endregion

        public List<Permission> Permissions => Value.ContainsKey("email_domains") ? Value["email_domains"].ToObject<List<Permission>>() : null;

        //  "format": {
        // "page_full_width": true
        // },

        //collection_view_page
        // "format": {
        // "page_icon": "📓",
        // "block_locked": false,
        // "block_locked_by": "d0a8cba1-b998-46a3-bcd1-0efb420410b1"
        // },

        public long CreatedTime => (long)Value["created_time"];
        public long LastEditedTime => (long)Value["last_edited_time"];
        public Guid CreatedBy => (Guid)Value["created_by"];
        public Guid LastEditedBy => (Guid)Value["last_edited_by"];
        public Guid ParentId => (Guid)Value["parent_id"];
        public bool Alive => (bool)Value["alive"];
        public Guid CreatedById => (Guid)Value["created_by_id"];
        public Guid LastEditedById => (Guid)Value["last_edited_by_id"];
        /// <summary>
        /// collection_view_page only
        /// </summary>
        public Guid CopiedFrom => (Guid)Value["copied_from"];
        public ParentTable ParentTable => Value.ContainsKey("parent_table") ? Value["parent_table"].ToObject<ParentTable>() : ParentTable.Unknown;
        // "created_by_table": "notion_user",
        // "last_edited_by_table": "notion_user",
    }
}
