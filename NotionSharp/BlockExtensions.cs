using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using NotionSharp.Lib.ApiV3.Model;

namespace NotionSharp
{
    public static class BlockExtensions
    {
        /// <summary>
        /// Shortcut of GetHtml to get an extract containing only an abstract, for rss feeds.
        /// </summary>
        /// <param name="recordMap">the record map containing page data</param>
        /// <param name="pageId">the id of the page to extract. Set to null to get the 1st block.</param>
        public static string GetHtmlAbstract(this RecordMap recordMap, Guid pageId = default)
            => recordMap.GetHtml(pageId, throwIfBlockMissing: false, stopBeforeFirstSubHeader: true);

        /// <summary>
        /// Get an HTML extract of the page
        /// </summary>
        /// <param name="recordMap">the record map containing page data</param>
        /// <param name="pageId">the id of the page to extract. Set to null to get the 1st block.</param>
        /// <param name="maxBlocks">set to the max number of blocks to process</param>
        /// <param name="throwIfBlockMissing">false to prevent an exception if a block is missing from the record map</param>
        /// <param name="stopBeforeFirstSubHeader">true to return only all html before the first sub-header</param>
        /// <returns></returns>
        public static string GetHtml(this RecordMap recordMap, Guid pageId = default, int maxBlocks = 0, bool throwIfBlockMissing = true, bool stopBeforeFirstSubHeader = false)
        {
            var pageBlock = pageId != default ? 
                recordMap.Block[pageId] 
                : recordMap.Block.First(b => b.Value.Type == "page").Value;

            //Exclude quote, code, page, bookmark
            var acceptedBlockTypes = new List<string> { "text", "image", "sub_header" };

            var blocks = from itemId in pageBlock.Content
                let block = !throwIfBlockMissing && !recordMap.Block.ContainsKey(itemId) ? null : recordMap.Block[itemId]
                where block != null && acceptedBlockTypes.Contains(block.Type)
                select block;

            if (maxBlocks > 0)
                blocks = blocks.Take(maxBlocks);

            if(stopBeforeFirstSubHeader)
                blocks = blocks.TakeWhile(block => block.Type != "sub_header");

            var html = (from block in blocks
                    let separator = block.Type == "sub_header" ? "h2" : "p"
                    select (separator, block: block.ToHtml())
                ).Aggregate(new StringBuilder(), (sb, tuple) =>
                    sb.Append("<").Append(tuple.separator).Append(">")
                        .Append(tuple.block)
                        .Append("</").Append(tuple.separator).AppendLine(">"))
                .ToString();

            return html;
        }

        /// <summary>
        /// Convert a block to an html representation
        /// </summary>
        /// <remarks>
        /// All block types: quote, code, page, bookmark, "text", "image", "sub_header"
        ///
        /// Supported block types: "text", "image", "sub_header"
        /// </remarks>
        public static string ToHtml(this Block block, bool throwIfNotSupported = true)
        {
            var sb = new StringBuilder();

            return block.Type switch
            {
                "text" => AppendText(sb, block).ToString(),
                "sub_header" => AppendText(sb, block).ToString(),
                "image" => AppendImage(sb, block).ToString(),
                _ => throwIfNotSupported ? throw new NotSupportedException($"block type {block.Type} not supported") : String.Empty
            };
        }

        public static StringBuilder AppendImage(this StringBuilder mainSb, Block imageBlock)
        {
            if (imageBlock.Type != "image")
                throw new ArgumentException($"textBlock.Type must be image, currently is {imageBlock.Type}", nameof(imageBlock));

            if (imageBlock.Properties != null && imageBlock.Properties.ContainsKey("format"))
            {
                var imageFormat = imageBlock.Properties["format"].ToObject<BlockImageFormat>();
                mainSb.Append("<img width=\"").Append(imageFormat.BlockWidth).Append("\" src=\"").Append(imageFormat.DisplaySource).Append("\"/>");
            }

            return mainSb;
        }

        public static StringBuilder AppendText(this StringBuilder mainSb, Block textBlock)
        {
            if(textBlock.Type != "text" && textBlock.Type != "sub_header")
                throw new ArgumentException($"textBlock.Type must be text, currently is {textBlock.Type}", nameof(textBlock));

            if (textBlock.Properties != null && textBlock.Properties.ContainsKey("title"))
            {
                var textParts = ((JArray)textBlock.Properties["title"]).Cast<JArray>();

                foreach (var textPart in textParts)
                    mainSb.AppendTextPart(textPart);
            }

            return mainSb;
        }

        static StringBuilder AppendTextPart(this StringBuilder sb, JArray textPart)
        {
            if (textPart.Count == 1)
                return sb.AppendLine(WebUtility.HtmlEncode((string)textPart[0]));

            if (textPart.Count == 2)
            {
                if (textPart[1] is JArray subParts)
                {
                    if (subParts.Count == 1 && subParts[0] is JArray outerTag && outerTag.All(t => !(t is JArray)))
                    {
                        var outerTagValue = (string) outerTag[0];
                        if (outerTagValue == "a" && outerTag.Count == 2)
                        {
                            sb.Append("<a href=\"").Append(outerTag[1]).Append("\">")
                                .Append(WebUtility.HtmlEncode((string)textPart[0]))
                                .Append("</a>");
                        }
                        else if(outerTag.Count == 1 && (outerTagValue == "i" || outerTagValue == "b"))
                        {
                            sb.Append("<").Append(outerTagValue).Append(">")
                                .Append(WebUtility.HtmlEncode((string) textPart[0]))
                                .Append("</").Append(outerTagValue).Append(">");
                        }
                        else
                            throw new NotSupportedException($"unknown outer tag {outerTag[0]} with counts:{outerTag.Count}");
                    }
                    else
                        throw new NotSupportedException($"unknown subParts {subParts}");
                }
                else
                    throw new NotSupportedException($"unknown subParts structure {textPart[1]}");
            }
            else
                throw new NotSupportedException($"this decoder supports only 1 or 2 textParts. This block has {textPart.Count} parts. textPart: {textPart}");

            return sb;
        }
    }
}