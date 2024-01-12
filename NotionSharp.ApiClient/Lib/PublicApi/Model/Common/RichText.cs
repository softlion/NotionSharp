using System;
using System.Text.Json.Serialization;

namespace NotionSharp.ApiClient;

/// <summary>
/// https://developers.notion.com/reference/rich-text
/// </summary>
public class RichText
{
    public const string TypeText = "text";
    public const string TypeMention = "mention";
    public const string TypeEquation = "equation";
        
    #region common properties
    /// <summary>
    /// One of RichText.Type* value
    /// </summary>
    public string Type { get; init; }
        
    public string PlainText { get; init; }
        
    public string? Href { get; init; }
        
    [JsonPropertyName("annotations")] 
    public RichTextAnnotation? Annotation { get; init; }

    //[JsonIgnore]
    //public bool HasStyle => Annotation?.HasColor  == true;
    #endregion

    public RichTextText? Text { get; init; } //type=text
    public Mention? Mention { get; init; } //type=mention
    public RichTextEquation? Equation { get; init; } //type=equation
}

public class Mention
{
    /// <summary>
    /// user, page, database, date, template (todo), equation (todo) , link preview (todo)
    /// </summary>
    public string Type { get; init; }
        
    public User? User { get; init; } //type=user
    public PageRef? Page { get; init; } //type=page
    public DatabaseRef Database { get; init; } //type=database
    public DateProp Date { get; init; } //type=date
}

public class PageRef : IBlockId
{
    /// <summary>
    /// Page Id
    /// </summary>
    public string Id { get; init; }
}

public class DatabaseRef : IBlockId
{
    /// <summary>
    /// Database Id
    /// </summary>
    public string Id { get; init; }
}

public class DateProp
{
    public DateTimeOffset Start { get; init; }
    public DateTimeOffset? End { get; init; }
}

public class RichTextAnnotation
{
    public bool Bold { get; init; }
    public bool Italic { get; init; }
    public bool Strikethrough { get; init; }
    public bool Underline { get; init; }
    public bool Code { get; init; }

    public NotionColor? Color { get; init; }

    [JsonIgnore] 
    public bool HasAnnotation => Bold || Italic || Strikethrough || Underline || Code || (Color != null && Color != NotionColor.Default);

    [JsonIgnore] 
    public bool HasColor => Color != null && Color != NotionColor.Default;
}

public record RichTextText
{
    public string Content { get; init; }

    public RichTextLink? Link { get; init; }
}

public record RichTextLink(string Url)
{
    //public string Type { get; set; } = "url";
}

/// <summary>
/// Equation
/// </summary>
/// <param name="Expression">Latex expression</param>
public record RichTextEquation(string Expression);
