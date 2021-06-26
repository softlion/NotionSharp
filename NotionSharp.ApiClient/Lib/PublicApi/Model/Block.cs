using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NotionSharp.ApiClient
{
    public class Block : BaseObject, IBlockId
    {
        #region common props
        public sealed override string Object { get; set; } = "block";
        
        public string Id { get; set; }

        [JsonIgnore]
        public static readonly string[] Types = {"unsupported","paragraph", "heading_1", "heading_2", "heading_3", "bulleted_list_item", "numbered_list_item", "to_do", "toggle", "child_page"};

        /// <summary>
        /// One of Block.Types
        /// </summary>
        public string Type { get; set; }
        
        [JsonPropertyName("created_time")]
        public DateTimeOffset CreatedTime { get; set; }

        [JsonPropertyName("last_edited_time")]
        public DateTimeOffset LastEditedTime { get; set; }

        [JsonPropertyName("has_children")]
        public bool HasChildren { get; set; }
        #endregion

        #region Only one of those is set. Depends on Type.
        public BlockTextAndChildren? Paragraph { get; set; }
        [JsonPropertyName("heading_1")]
        public BlockText? Heading1 { get; set; }
        [JsonPropertyName("heading_2")]
        public BlockText? Heading2 { get; set; }
        [JsonPropertyName("heading_3")]
        public BlockText? Heading3 { get; set; }
        [JsonPropertyName("bulleted_list_item")]
        public BlockTextAndChildren? BulletedListItem { get; set; } 
        [JsonPropertyName("numbered_list_item")]
        public BlockTextAndChildren? NumberedListItem { get; set; } 
        [JsonPropertyName("to_do")]
        public BlockTextAndChildrenAndCheck? ToDo { get; set; }
        public BlockTextAndChildren? Toggle { get; set; }
        
        [JsonPropertyName("child_page")]
        public BlockChildPage? ChildPage { get; set; }
        #endregion
    }

    public class BlockText
    {
        public List<RichText> Text { get; set; }
    }

    public class BlockTextAndChildren : BlockText
    {
        public List<Block> Children { get; set; } 
    }

    public class BlockTextAndChildrenAndCheck : BlockTextAndChildren
    {
        public bool Checked { get; set; } 
    }

    public class BlockChildPage
    {
        public string Title { get; set; }
    }
}
