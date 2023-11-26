using System.ComponentModel;
using System.Text;

namespace NotionSharp.ApiClient.Lib.HtmlRendering;

public static class HtmlBlockExtensions
{
    public static string? GetColor(this NotionColor color)
    {
        //TODO: replace tolower by ToSnakeCase
        return ((string?)TypeDescriptor.GetConverter(color).ConvertTo(color, typeof(string)))?.ToLower();
    }
    
    /// <summary>
    /// TODO: switch to https://github.com/twitter/twemoji
    /// </summary>
    /// <param name="emojiString">an emoji string</param>
    /// <returns>A url of a svg file</returns>
    public static StringBuilder? GetTwitterEmojiUrl(this string emojiString)
    {
        var enc = new UTF32Encoding(true, false);
        var bytes = enc.GetBytes(emojiString);

        var sbCodepointEmoji = new StringBuilder();
        for (var i = 0; i < bytes.Length; i += 4)
        {
            var value = bytes[i] << 24 | bytes[i + 1] << 16 | bytes[i + 2] << 8 | bytes[i + 3];
            if (value is 0xFE0E or 0xFE0F)
                continue;
            sbCodepointEmoji.Append($"{value:x}-");
        }

        if (sbCodepointEmoji.Length > 0 && sbCodepointEmoji[^1] == '-')
            sbCodepointEmoji.Remove(sbCodepointEmoji.Length - 1, 1);

        if (sbCodepointEmoji.Length == 0)
            return null;

        sbCodepointEmoji.Insert(0, "https://cdn.jsdelivr.net/gh/twitter/twemoji/assets/svg/");
        sbCodepointEmoji.Append(".svg");
        return sbCodepointEmoji;
    }
}