#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NotionSharp.Lib.ApiV3.Model;

namespace NotionSharp
{
    public static class HtmlBlockExtensions
    {
        /// <summary>
        /// Shortcut of GetHtml to get an extract containing only an abstract, for rss feeds.
        /// </summary>
        /// <param name="recordMap">the record map containing page data</param>
        /// <param name="pageId">the id of the page to extract. Set to null to get the 1st block.</param>
        public static string GetHtmlAbstract(this RecordMap recordMap, Guid pageId = default)
            => recordMap.GetHtml(pageId, throwIfBlockMissing: false, stopBeforeFirstSubHeader: true, throwIfCantDecodeTextData: false);

        /// <summary>
        /// Get an HTML extract of the page
        /// </summary>
        /// <param name="recordMap">the record map containing page data</param>
        /// <param name="pageId">the id of the page to extract. Set to null to get the 1st block.</param>
        /// <param name="maxBlocks">set to the max number of blocks to process</param>
        /// <param name="throwIfBlockMissing">false to prevent an exception if a block is missing from the record map</param>
        /// <param name="stopBeforeFirstSubHeader">true to return only all html before the first sub-header</param>
        /// <param name="throwIfCantDecodeTextData"></param>
        /// <returns></returns>
        public static string GetHtml(this RecordMap recordMap, Guid pageId = default, int maxBlocks = 0, bool throwIfBlockMissing = true, bool stopBeforeFirstSubHeader = false, bool throwIfCantDecodeTextData = true)
        {
            var sb = new StringBuilder();

            TransformOptions transformOptions = null!;
            transformOptions = new TransformOptions
            {
                //Exclude code, page, bookmark
#if !DEBUG
                AcceptedBlockTypes = new List<string> { "text", "header", "sub_header", "sub_sub_header", "bulleted_list", "image", "quote", "column_list", "column", "callout" },
#endif
                ThrowIfBlockMissing = throwIfBlockMissing,
                ThrowIfCantDecodeTextData = throwIfCantDecodeTextData,
                MaxBlocks = maxBlocks,
                TransformHeader = (data, block) =>
                {
                    sb.Append("<h1 class=\"notion-header-block\">").AppendText(data).AppendLine("</h1>");
                    return true;
                },
                TransformSubHeader = (data, block) =>
                {
                    if (stopBeforeFirstSubHeader)
                        return false;
                    sb.Append("<h2 class=\"notion-sub_header-block\">").AppendText(data).AppendLine("</h2>");
                    return true;
                },
                TransformSubSubHeader = (data, block) =>
                {
                    sb.Append("<h3 class=\"notion-sub_sub_header-block\">").AppendText(data).AppendLine("</h3>");
                    return true;
                },
                TransformText = (data, block) =>
                {
                    //if (data != null)
                        sb.Append("<div class=\"notion-text-block\">").AppendText(data).AppendLine("</div>");
                    return true;
                },
                TransformBulletedList = (data, block) =>
                {
                    sb.Append("<ul class=\"notion-bulleted_list-block\"><li>").AppendText(data).AppendLine("</li>");
                    return (true, () => sb.AppendLine("</ul>"));
                },
                TransformQuote = (data, block) =>
                {
                    sb.Append("<div class=\"notion-quote-block\">").AppendText(data).AppendLine("</div>");
                    return true;
                },
                TransformImage = (data, block) =>
                {
                    if (data != null)
                        sb.Append("<div class=\"notion-image-block\">").AppendImage(data, block.Id).AppendLine("</div>");

                    return true;
                },
                TransformColumnList = (data, block) =>
                {
                    //Start of notion-column_list-block
                    sb.Append("<div class=\"notion-column_list-block\"><div style=\"display: flex\">");
                    var totalColumns = data.Columns.Count;

                    return (true,
                        StartColumn: (columnIndex, column) =>
                        {
                            sb.Append(FormattableString.Invariant($"<div class=\"notion-column\" style=\"width: calc((100% - {46*(totalColumns-1)}px) * {column.Ratio});\">"));
                            return () => sb.Append("</div>");
                        },
                        TransformColumnSeparator: columnIndex => sb.Append("<div class=\"notion-column-separator\"><div class=\"notion-column-separator-line\"></div></div>"),
                        EndColumnList: () => sb.Append("</div></div>"));
                },
                TransformCallout = (data, block) =>
                {
                    sb.AppendLine(@$"<div class=""notion-callout-block notion-block-color-{data.Format.BlockColor}"">");

                    if (Uri.TryCreate(data.Format.PageIcon, UriKind.Absolute, out var iconUrl))
                        sb.AppendLine(@$"<img class=""notion-icon"" src=""{iconUrl}"" />");
                    else
                        sb.AppendLine(@$"<img class=""notion-emoji"" src=""{GetTwitterEmojiUrl(data.Format.PageIcon)}"" />");
                    
                    transformOptions.TransformText(data.Text, block);
                    sb.AppendLine("</div>");
                    return true;
                },
#if DEBUG
                TransformOther = block =>
                {
                    return true;
                }
#endif
            };

            recordMap.Transform(transformOptions, pageId);
            return sb.ToString();
        }

        public static StringBuilder AppendText(this StringBuilder sb, BlockTextData data)
        {
            if (data != null)
            {
                foreach (var line in data.Lines.Where(l => l != null))
                {
                    var tag = line.HasProperty ? (line.HyperlinkUrl != null ? "a" : "span") : null;

                    if (tag != null)
                    {
                        sb.Append("<").Append(tag);

                        if (line.HyperlinkUrl != null)
                            sb.Append(" href=\"").Append(Uri.EscapeUriString(line.HyperlinkUrl)).Append("\"");

                        if (line.HasStyle)
                        {
                            sb.Append(" style=\"");
                            if (line.HtmlColor != null)
                                sb.Append("color: ").Append(line.HtmlColor).Append(";");
                            if (line.IsBold)
                                sb.Append("font-weight: 600;");
                            if (line.IsItalic)
                                sb.Append("font-style: italic;");
                            sb.Append("\"");
                        }

                        sb.Append(">").Append(WebUtility.HtmlEncode(line.Text)).Append("</").Append(tag).Append(">");
                    }
                    else
                    {
                        sb.Append(WebUtility.HtmlEncode(line.Text));
                    }
                }
            }

            return sb;
        }

        public static StringBuilder AppendImage(this StringBuilder sb, BlockImageData data, Guid blockId)
        {
            if (data != null)
            {
                var imageUrl = $"{data.ImageUrl}?table=block&id={blockId:D}&cache=v2";
                if (data.Format != null)
                    sb.Append("<img style=\"width:").Append(data.Format.BlockWidth).Append("px\" src=\"").Append(imageUrl).Append("\"/>");
                else
                    sb.Append("<img src=\"").Append(imageUrl).Append("\"/>");

                if (!String.IsNullOrWhiteSpace(data.Caption))
                    sb.Append("<div class=\"notion-text-block notion-image-caption\">").Append(data.Caption).Append("</div>");
            }

            return sb;
        }

        public static StringBuilder? GetTwitterEmojiUrl(string emojiString)
        {
            var enc = new UTF32Encoding(true, false);
            var bytes = enc.GetBytes(emojiString);

            var sbCodepointEmoji = new StringBuilder();
            for (var i = 0; i < bytes.Length; i += 4)
                sbCodepointEmoji.Append($"{bytes[i]:x2}{bytes[i+1]:x2}{bytes[i+2]:x2}{bytes[i+3]:x2}-".TrimStart('0'));

            if (sbCodepointEmoji.Length > 0 && sbCodepointEmoji[^1] == '-')
                sbCodepointEmoji.Remove(sbCodepointEmoji.Length - 1, 1);

            if (sbCodepointEmoji.Length == 0)
                return null;

            sbCodepointEmoji.Insert(0, "//cdn.jsdelivr.net/gh/twitter/twemoji/assets/svg/");
            sbCodepointEmoji.Append(".svg");
            return sbCodepointEmoji;
        }
    }
}
