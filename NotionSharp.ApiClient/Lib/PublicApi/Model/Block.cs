using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NotionSharp.ApiClient;

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
    public const string Image = "image";

    public static readonly string[] Types =
    {
        Unsupported, Paragraph, Heading1, Heading2, Heading3, BulletedListItem, NumberedListItem, ToDo, Toggle, ChildPage,
        Image
    };
}

public class Block : NamedObject, IBlockId
{
    #region common props
    public Block()
    {
        Object = "block";
    }
        
    public string Id { get; init; }
    public Parent Parent { get; init; }

    /// <summary>
    /// One of BlockTypes.*
    /// </summary>
    public string Type { get; init; }
        
    public DateTimeOffset CreatedTime { get; init; }

    public DateTimeOffset LastEditedTime { get; init; }

    public bool HasChildren { get; init; }
    #endregion

    #region Only one of those is set. Depends on Type.
    public BlockTextAndChildren? Paragraph { get; init; }
    [JsonPropertyName("heading_1")]
    public BlockText? Heading1 { get; init; }
    [JsonPropertyName("heading_2")]
    public BlockText? Heading2 { get; init; }
    [JsonPropertyName("heading_3")]
    public BlockText? Heading3 { get; init; }
    public BlockTextAndChildren? BulletedListItem { get; init; } 
    public BlockTextAndChildren? NumberedListItem { get; init; } 
    public BlockTextAndChildrenAndCheck? ToDo { get; init; }
    public BlockTextAndChildren? Toggle { get; init; }
        
    public BlockChildPage? ChildPage { get; init; }
        
    //[JsonPropertyName("image")]
    public BlockImage? Image { get; init; }
    #endregion
}

public record BlockText([property: JsonPropertyName("rich_text")] List<RichText> RichText);

public record BlockTextAndChildren(List<Block> Children, List<RichText> RichText) : BlockText(RichText);
public record BlockTextAndChildrenAndCheck(List<Block> Children, List<RichText> RichText) : BlockTextAndChildren(Children, RichText)
{
    public bool Checked { get; init; }
}
public record BlockChildPage(string Title);
public record BlockImage(External? External);
public record External(string Url);