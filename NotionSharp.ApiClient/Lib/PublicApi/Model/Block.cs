using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NotionSharp.ApiClient
{
    public static class BlockTypes
    {
        public const string Unsupported = "unsupported";
        public const string Paragraph = "paragraph";
        public const string Heading1 = "heading_1";
        public const string Heading2 = "heading_2";
        public const string Heading3 = "heading_3";
        public const string BulletedListItem = "bulleted_list_item";
        public const string NumberedListItem = "numbered_list_item";
        public const string ToDo = "to_do";
        public const string Toggle = "toggle";
        public const string ChildPage = "child_page";

        public static readonly string[] Types = {Unsupported,Paragraph, Heading1, Heading2, Heading3, BulletedListItem, NumberedListItem, ToDo, Toggle, ChildPage};
    }
    
    public class Block : BaseObject, IBlockId
    {
        #region common props
        public sealed override string Object { get; set; } = "block";
        
        public string Id { get; set; }

        /// <summary>
        /// One of BlockTypes.*
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
