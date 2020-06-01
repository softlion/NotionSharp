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

            var transformOptions = new TransformOptions
            {
                //Exclude quote, code, page, bookmark
                AcceptedBlockTypes = new List<string> { "text", "header", "sub_header", "bulleted_list", "image" },
                ThrowIfBlockMissing = throwIfBlockMissing,
                ThrowIfCantDecodeTextData = throwIfCantDecodeTextData,
                MaxBlocks = maxBlocks,
                TransformHeader = (data, block) =>
                {
                    sb.Append("<h1 class='notion_header'>").AppendText(data).AppendLine("</h1>");
                    return true;
                },
                TransformSubHeader = (data, block) =>
                {
                    if (stopBeforeFirstSubHeader)
                        return false;
                    sb.Append("<h2 class='notion_sub_header'>").AppendText(data).AppendLine("</h2>");
                    return true;
                },
                TransformBulletedList = (data, block) =>
                {
                    sb.Append("<ul><li class='notion_bulleted_list'>").AppendText(data).AppendLine("</li></ul>");
                    var hasContent = block.Content?.Count > 0;
                    if(hasContent)
                        sb.Append("<p class='notion_bulleted_list_content'>");
                    return (true, () => { if(hasContent) sb.Append("</p>"); });
                },
                TransformText = (data, block) =>
                {
                    if (data != null)
                        sb.Append("<p class='notion_text'>").AppendText(data).AppendLine("</p>");
                    return true;
                },
                TransformImage = (data, block) =>
                {
                    if(data != null)
                        sb.Append("<p class='notion_image'>").AppendImage(data).AppendLine("</p>");
                    return true;
                }
            };

            recordMap.Transform(transformOptions, pageId);
            return sb.ToString();
        }

        ///// <summary>
        ///// Convert a block to an html representation
        ///// </summary>
        ///// <remarks>
        ///// Supported block types: "text", "image", "sub_header", "header", "bulleted_list"
        ///// </remarks>
        //public static string ToHtml(this Block block, bool throwIfNotSupported = true, bool throwIfCantDecodeText = true)
        //{
        //    var sb = new StringBuilder();

        //    return block.Type switch
        //    {
        //        "text" => sb.AppendText(block.ToTextData(throwIfCantDecodeText)).ToString(),
        //        "header" => sb.AppendText(block.ToTextData(throwIfCantDecodeText)).ToString(),
        //        "sub_header" => sb.AppendText(block.ToTextData(throwIfCantDecodeText)).ToString(),
        //        "bulleted_list" => sb.AppendText(block.ToTextData(throwIfCantDecodeText)).ToString(),
        //        "image" => sb.AppendImage(block.ToImageData()).ToString(),
        //        _ => throwIfNotSupported ? throw new NotSupportedException($"block type {block.Type} not supported") : String.Empty
        //    };
        //}

        static StringBuilder AppendText(this StringBuilder sb, BlockTextData data)
        {
            foreach(var line in data.Lines.Where(l => l != null))
            {
                var tag = line.HasProperty ? (line.HyperlinkUrl != null ? "a" : "span") : null;

                if(tag != null)
                {
                    sb.Append("<").Append(tag);

                    if (line.HyperlinkUrl != null)
                        sb.Append(" href=\"").Append(Uri.EscapeUriString(line.HyperlinkUrl)).Append("\"");

                    if(line.HasStyle)
                    {
                        sb.Append(" style=\"");
                        if (line.HtmlColor != null)
                            sb.Append("color: ").Append(line.HtmlColor).Append(";");
                        if(line.IsBold)
                            sb.Append("font-weight: bold;");
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

            return sb;
        }

        static StringBuilder AppendImage(this StringBuilder sb, BlockImageData data)
        {
            if (data != null)
            {
                if (data.Format != null)
                    sb.Append("<img width=\"").Append(data.Format.BlockWidth).Append("\" src=\"").Append(data.ImageUrl).Append("\"/>");
                else
                    sb.Append("<img src=\"").Append(data.ImageUrl).Append("\"/>");
            }

            return sb;
        }
    }
}
