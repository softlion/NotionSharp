using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using NotionSharp.ApiClient.Lib.Helpers;

namespace NotionSharp.ApiClient;

/// <summary>
/// https://developers.notion.com/reference/block
/// </summary>
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
    public const string Quote = "quote";
    public const string File = "file";
    public const string Callout = "callout";
    public const string ColumnList = "column_list";
    public const string Column = "column";

    public static readonly string[] SupportedBlocks =
    {
        BulletedListItem, Callout, //ChildDatabase, 
        //ChildPage, 
        ColumnList, Column,
        //When the is_toggleable property is true
        Heading1, Heading2, Heading3,
        NumberedListItem, Paragraph, Image, Quote, File, //SyncedBlock, Table, Template,
        ToDo, Toggle
    };

    public static readonly string[] BlocksWithChildren =
    {
        BulletedListItem, Callout, //ChildDatabase, 
        ChildPage, ColumnList, Column,
        //When the is_toggleable property is true
        Heading1, Heading2, Heading3,
        NumberedListItem, Paragraph, Quote, //SyncedBlock, Table, Template,
        ToDo, Toggle
    };
}

sealed class MyJsonStringEnumConverter() : JsonStringEnumConverter<NotionColor>(JsonNamingPolicy.SnakeCaseLower);

[JsonConverter(typeof(MyJsonStringEnumConverter))]
public enum NotionColor
{
    Default,
    Blue,
    BlueBackground,
    Brown,
    BrownBackground,
    Gray,
    GrayBackground,
    Green,
    GreenBackground,
    Orange,
    OrangeBackground,
    Yellow,
    YellowBackground,
    Pink,
    PinkBackground,
    Purple,
    PurpleBackground,
    Red,
    RedBackground,
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
    
    /// <summary>
    /// Partial User
    /// </summary>
    public User CreatedBy { get; init; }

    public DateTimeOffset LastEditedTime { get; init; }
    public User LastEditedBy { get; init; }

    public bool Archived { get; init; }
    public bool HasChildren { get; init; }
    
    [JsonIgnore]
    public List<Block> Children { get; set; }
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
        
    public NotionFile? Image { get; init; }
    public NotionFile? File { get; init; }
    public BlockTextAndChildrenAndColor? Quote { get; set; }
    public BlockCallout? Callout { get; init; }
    #endregion
}

public record BlockText(List<RichText> RichText);

public record BlockTextAndChildren(List<Block> Children, List<RichText> RichText) : BlockText(RichText);
public record BlockTextAndChildrenAndCheck(List<Block> Children, List<RichText> RichText) : BlockTextAndChildren(Children, RichText)
{
    public bool Checked { get; init; }
}
public record BlockTextAndChildrenAndColor(List<Block> Children, List<RichText> RichText) : BlockTextAndChildren(Children, RichText)
{
    public NotionColor Color { get; init; }
}
public record BlockChildPage(string Title);

public record BlockCallout(List<RichText> RichText, NotionColor Color, NotionIcon Icon)
{
    /// <summary>
    /// File or Emoji
    /// </summary>
    public NotionIcon Icon { get; init; } = Icon;
}

#region File and Emoji
public record NotionFile(string Type, NotionFileContent? File, NotionFileExternal? External) : NotionIcon(Type);
public record NotionFileExternal(string Url);
public record NotionFileContent(string Url, string ExpiryTime);

public record NotionEmoji(string Type, string Emoji) : NotionIcon(Type);
#endregion

[JsonConverter(typeof(BufferedJsonPolymorphicConverterFactory))]
[BufferedJsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[BufferedJsonDerivedType(typeof(NotionFile), "file")]
[BufferedJsonDerivedType(typeof(NotionEmoji), "emoji")]
public abstract record NotionIcon(string Type);
