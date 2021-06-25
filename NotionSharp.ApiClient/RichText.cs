using System;
using System.Text.Json.Serialization;

namespace NotionSharp.ApiClient
{
    /// <summary>
    /// https://developers.notion.com/reference/rich-text
    /// </summary>
    //[JsonConverter(typeof(RichTextJsonConverter))]
    public class RichText
    {
        public const string TypeText = "text";
        public const string TypeMention = "mention";
        public const string TypeLink = "link";
        public const string TypeEquation = "equation";
        
        /// <summary>
        /// One of RichText.Type* value
        /// </summary>
        public string Type { get; set; }
        
        [JsonPropertyName("plain_text")] 
        public string PlainText { get; set; }
        
        public string? Href { get; set; }
        
        [JsonPropertyName("annotations")] 
        public RichTextAnnotation Annotation { get; set; }

        public RichTextText? Text { get; set; } //type=text
        public string? Url { get; set; } //type=link
        public Mention? Mention { get; set; } //type=mention
        public RichTextEquation? Equation { get; set; } //type=equation
    }

    public class Mention
    {
        /// <summary>
        /// user, page, database, date
        /// </summary>
        public string Type { get; set; }
        
        public User? User { get; set; } //type=user
        public PageRef? Page { get; set; } //type=page
        public DatabaseRef Database { get; set; } //type=database
        public DateProp Date { get; set; } //type=date
    }

    public class PageRef
    {
        /// <summary>
        /// Page Id
        /// </summary>
        public string Id { get; set; }
    }

    public class DatabaseRef
    {
        /// <summary>
        /// Database Id
        /// </summary>
        public string Id { get; set; }
    }

    public class DateProp
    {
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset? End { get; set; }
    }

    public class RichTextAnnotation
    {
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Strikethrough { get; set; }
        public bool Underline { get; set; }
        public bool Code { get; set; }

        /// <summary>
        /// One of RichTextAnnotationColors.Colors
        /// </summary>
        public string? Color { get; set; }
    }

    public static class RichTextAnnotationColors
    {
        public static readonly string[] Colors = {"default", "gray", "brown", "orange", "yellow", "green", "blue", "purple", "pink", "red", "gray_background", "brown_background", "orange_background", "yellow_background", "green_background", "blue_background", "purple_background", "pink_background", "red_background"};
    }

    public class RichTextText
    {
        public string Content { get; set; }
        public RichTextLink? Link { get; set; }
    }

    public class RichTextLink
    {
        public string Type { get; set; } = "url";
        public string Url { get; set; }
    }

    public class RichTextEquation
    {
        /// <summary>
        /// LATEX expression
        /// </summary>
        public string Expression { get; set; }
    }
}