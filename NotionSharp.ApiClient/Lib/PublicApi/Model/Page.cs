using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using NotionSharp.ApiClient.Lib.Helpers;

namespace NotionSharp.ApiClient;

public static class PageExtensions
{
    /// <summary>
    /// If false, then the only property in Properties is "title"
    /// </summary>
    public static bool ParentIsDatabase(this Page page) => page.Parent is PageParentDatabase;
    /// <summary>
    /// If true, it is a top level page
    /// </summary>
    public static bool ParentIsWorkspace(this Page page) => page.Parent is PageParentWorkspace;

    public static TitlePropertyItem? Title(this Page page)
    {
        if (page.Properties?.TryGetValue("title", out var item) == true && item is TitlePropertyItem title)
            return title;
        return null;
    }
}

public class Page : NamedObject, IBlockId
{
    public Page()
    {
        Object = "page";
    }
    public string Id { get; init; }
            
    public DateTimeOffset CreatedTime { get; init; }
    public DateTimeOffset LastEditedTime { get; init; }
    public bool Archived { get; init; }
    
    public User CreatedBy { get; init; }
    public User LastEditedBy { get; init; }

    public string Url { get; init; }
    public string PublicUrl { get; init; }

    public PageParent Parent { get; init; }


    /// <remarks>
    /// Partial, instead use https://developers.notion.com/reference/retrieve-a-page-property 
    /// If parent.type is "page_id" or "workspace", then the only valid key is "title".
    /// If parent.type is "database_id", then the keys and values of this field are determined by the properties of the database this page belongs to.
    /// </remarks>
    public Dictionary<string, PropertyItem>? Properties { get; init; }

    
    

    //TODO: polymorph converter (for type=database)
    //See https://developers.notion.com/reference/page#page-property-value
    // public abstract class PagePropertyValue : BaseObject
    // {
    //     /// <summary>
    //     /// Possible values are:
    //     /// "rich_text", "number", "select", "multi_select", "date", "formula", "relation", "rollup", "title", "people", "files", "checkbox", "url", "email", "phone_number", "created_time", "created_by", "last_edited_time", "last_edited_by".
    //     /// </summary>
    //     public string Type { get; set; }
    // }
}

#region Parent polymorphism

[JsonConverter(typeof(BufferedJsonPolymorphicConverterFactory))]
[BufferedJsonPolymorphic(
    TypeDiscriminatorPropertyName = "type",
    IgnoreUnrecognizedTypeDiscriminators = true,
    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
[BufferedJsonDerivedType(typeof(PageParentWorkspace), TypeWorkspace)]
[BufferedJsonDerivedType(typeof(PageParentPage), TypePage)]
[BufferedJsonDerivedType(typeof(PageParentDatabase), TypeDatabase)]
[BufferedJsonDerivedType(typeof(PageParentBlock), TypeBlock)]
public record PageParent(string Type)
{
    public const string TypeWorkspace = "workspace";
    public const string TypePage = "page_id";
    public const string TypeDatabase = "database_id";
    public const string TypeBlock = "block_id";
}
public record PageParentWorkspace(string Type) : PageParent(Type);
public record PageParentPage(string Type, string PageId) : PageParent(Type);
public record PageParentDatabase(string Type, string DatabaseId) : PageParent(Type);
public record PageParentBlock(string Type, string DatabaseId) : PageParent(Type);
#endregion
