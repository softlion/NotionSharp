using System.Text.Json;
using System.Text.Json.Serialization;

namespace NotionSharp.ApiClient
{
    /// <summary>
    /// https://developers.notion.com/reference/rich-text
    /// </summary>
    //[JsonConverter(typeof(RichTextJsonConverter))]
    public class RichText
    {
        //"text", "mention", "link", "equation".
        public string Type { get; set; }
        [JsonPropertyName("plain_text")] public string PlainText { get; set; }
        public string? Href { get; set; }
        [JsonPropertyName("annotations")] public RichTextAnnotation Annotation { get; set; }

        public RichTextText? Text { get; set; } //type=text
        public string? Url { get; set; } //type=link
        public JsonElement? Mention { get; set; } //type=mention
        public RichTextEquation? Equation { get; set; } //type=equation
    }

    public class RichTextAnnotation
    {
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Strikethrough { get; set; }
        public bool Underline { get; set; }
        public bool Code { get; set; }

        /// <summary>
        /// "default", "gray", "brown", "orange", "yellow", "green", "blue", "purple", "pink", "red", "gray_background", "brown_background", "orange_background", "yellow_background", "green_background", "blue_background", "purple_background", "pink_background", "red_background".
        /// </summary>
        public string? Color { get; set; }
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