using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace NotionSharp
{
    public static class NotionUtils
    {
        //RegexOptions options = RegexOptions.None;
        static readonly Regex TrimSpaces = new (@"[\s-:()/\\\W]+", RegexOptions.None);
        //sentence = regex.Replace(sentence, " ");

        public static Uri GetPageUri(string pageId, string? title, Uri baseUrl)
        {
            var pageTitle = Uri.EscapeDataString(TrimSpaces.Replace(title ?? "", "-"));
            pageId = pageId.Replace("-", string.Empty);
            return new(baseUrl, $"{pageTitle}-{pageId}");
        }

        /// <summary>
        /// Extract the block/page ID from a Notion.so URL
        /// if it's a bare page URL, it will be the ID of the page.
        /// If there's a hash with a block ID in it (from clicking "Copy Link") on a block in a page), it will instead be the ID of that block.
        /// If it's already in ID format, it will be passed right through.
        /// </summary>
        /// <example>
        /// https://www.notion.so/vapolia/Secret-feature-Xamarin-Forms-control-s-auto-registration-1fd6f1b0d98d4aabb2defa0eb14961fa
        /// https://www.notion.so/vapolia/Secret-feature-Xamarin-Forms-control-s-auto-registration-1fd6f1b0d98d4aabb2defa0eb14961fa#1678f28a66d7486b93c4c98a1caf7967
        /// </example>
        public static Guid? ExtractId(string urlOrId)
        {
            if (urlOrId.StartsWith("http"))
                urlOrId = new Uri(urlOrId).GetLeftPart(UriPartial.Path).Split('-').Last().Split('#').Last();

            if (Guid.TryParse(urlOrId, out var guid))
                return guid;

            return null;
        }
    }
}
