using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotionSharp.ApiClient.Lib.HtmlRendering
{
    public class HtmlRenderer
    {
        /// <summary>
        /// Get an HTML extract of the page
        /// </summary>
        /// <param name="blocks">the page's child blocks</param>
        /// <param name="stopBeforeFirstSubHeader">true to return only all html before the first sub-header</param>
        /// <returns>An HTML string</returns>
        public virtual string GetHtml(IEnumerable<Block> blocks, bool stopBeforeFirstSubHeader = false)
        {
            var sb = new StringBuilder();

            foreach (var block in blocks)
                if(!Transform(block, sb, stopBeforeFirstSubHeader))
                    break;

            return sb.ToString();
        }

        protected virtual bool Transform(Block? block, StringBuilder sb, bool stopBeforeFirstSubHeader)
        {
            if (block == null)
                return true;
            
            switch (block.Type)
            {
                case BlockTypes.Heading1:
                    TransformHeading1(block, sb);
                    break;
                case BlockTypes.Heading2:
                    if (stopBeforeFirstSubHeader)
                        return false;
                    TransformHeading2(block, sb);
                    break;
                case BlockTypes.Heading3:
                    TransformHeading3(block, sb);
                    break;
                case BlockTypes.Paragraph:
                    TransformParagraph(block, sb);
                    break;
                case BlockTypes.ChildPage:
                    //block.ChildPage
                    //TODO
                    break;
                case BlockTypes.ToDo:
                    //TODO input type=radio
                    sb.Append("<ul><li>");
                    Append(block.ToDo, sb);
                    sb.AppendLine("</li></ul>");
                    break;
                case BlockTypes.Toggle:
                    //TODO input type=checkbox
                    sb.Append("<ul><li>");
                    Append(block.Toggle, sb);
                    sb.AppendLine("</li></ul>");
                    break;
                case BlockTypes.BulletedListItem:
                    TransformBulletedListItem(block, sb);
                    break;
                case BlockTypes.NumberedListItem:
                    TransformNumberedListItem(block, sb);
                    break;
                case BlockTypes.Image:
                    TransformImage(block.Image, block.Id, sb);
                    break;
                
                case BlockTypes.Unsupported:
                    break;
#if DEBUG
                default:
                    throw new ArgumentException($"Unknown block type {block.Type}");
#endif
            }

            return true;
        }

        protected virtual void TransformImage(BlockImage data, string blockId, StringBuilder sb)
        {
            if (data.External != null)
            {
                sb.Append("<div class=\"notion-image-block\">");
                var imageUrl = $"{data.External?.Url}?table=block&id={blockId}&cache=v2";
                sb.Append("<img src=\"").Append(imageUrl).Append("\"/>");
                sb.AppendLine("</div>");
            }
        }
        
        protected virtual void TransformBulletedListItem(Block block, StringBuilder sb)
        {
             sb.Append("<ul><li>");
             Append(block.BulletedListItem, sb);
             sb.AppendLine("</li></ul>");
        }

        protected virtual void TransformNumberedListItem(Block block, StringBuilder sb)
        {
            sb.Append("<ol><li>");
            Append(block.NumberedListItem, sb);
            sb.AppendLine("</li></ol>");
        }

        protected virtual void TransformHeading1(Block block, StringBuilder sb)
        {
            sb.Append("<h1>");
            Append(block.Heading1?.RichText, sb).AppendLine("</h1>");
        }
        protected virtual void TransformHeading2(Block block, StringBuilder sb)
        {
            sb.Append("<h2>");
            Append(block.Heading1?.RichText, sb).AppendLine("</h2>");
        }
        protected virtual void TransformHeading3(Block block, StringBuilder sb)
        {
            sb.Append("<h3>");
            Append(block.Heading1?.RichText, sb).AppendLine("</h3>");
        }
        protected virtual void TransformParagraph(Block block, StringBuilder sb)
        {
            sb.Append("<div class=\"notion-paragraph\">");
            Append(block.Paragraph, sb);
            sb.AppendLine("</div>");
        }
        
                
        protected virtual void Append(BlockTextAndChildren? block, StringBuilder sb)
        {
            Append(block?.RichText, sb);
            if (block?.Children != null)
            {
                foreach (var child in block.Children)
                    Transform(child, sb, false);
            }
        }
        
        protected virtual StringBuilder Append(List<RichText>? data, StringBuilder sb)
        {
            if (data == null) 
                return sb;
            
            foreach (var line in data.Where(l => l != null))
                Append(line, sb);

            return sb;
        }

        protected virtual StringBuilder Append(RichText? line, StringBuilder sb)
        {
            if (line == null)
                return sb;

            sb.Append("<div class=\"notion-line\">");
            var tag = line.HasAttribute ? (line.Href != null ? "a" : "span") : null;

            if (tag != null)
            {
                sb.Append("<").Append(tag);

                if (line.Href != null)
                    sb.Append(" href=\"").Append(Uri.EscapeUriString(line.Href)).Append("\"");

                if (line.HasStyle)
                {
                    sb.Append(" class=\"");
                    if (line.Annotation.Bold)
                        sb.Append(" notion-bold");
                    if (line.Annotation.Italic)
                        sb.Append(" notion-italic");
                    if (line.Annotation.Strikethrough)
                        sb.Append(" notion-strikethrough");
                    if (line.Annotation.Underline)
                        sb.Append(" notion-underline");
                    if (line.Annotation.Color != null)
                        sb.Append(" notion-color-").Append(line.Annotation.Color);
                    if (line.Annotation?.Code != null)
                        sb.Append(" notion-code");
                    sb.Append("\"");
                }

                sb.Append(">");
                switch (line.Type)
                {
                    case RichText.TypeText:
                        Append(line.Text, sb);
                        break;
                    case RichText.TypeEquation:
                        Append(line.Equation, sb);
                        break;
                    case RichText.TypeLink:
                        AppendUrl(line.Url, sb);
                        break;
                    case RichText.TypeMention:
                        Append(line.Mention, sb);
                        break;
#if DEBUG
                    default:
                        throw new ArgumentException($"Unknown RichText type {line.Type}");
#endif
                }
                sb.Append("</").Append(tag).Append(">");
            }
            else
            {
                Append(line.Text, sb);
            }
            sb.Append("</div>");

            return sb;
        }

        protected virtual StringBuilder Append(RichTextText? text, StringBuilder sb)
        {
            if (text == null)
                return sb;

            var hasLink = text.Link?.Url != null;
            if (hasLink)
                sb.Append("<a href=\"").Append(Uri.EscapeUriString(text.Link.Url)).Append("\">");
            sb.Append(text.Content);
            if (hasLink)
                sb.Append("</a>");

            return sb;
        }


        protected virtual StringBuilder AppendUrl(string? url, StringBuilder sb)
        {
            if (url != null)
                sb.Append("<a href=\"").Append(Uri.EscapeUriString(url)).Append("\">").Append(url).Append("</a>");
            return sb;
        }

        protected virtual StringBuilder Append(RichTextEquation? equation, StringBuilder sb)
        {
            //TODO
            return sb;
        }
        
        protected virtual StringBuilder Append(Mention? mention, StringBuilder sb)
        {
            //TODO
            return sb;
        }
    }
    
    
//                 //Exclude code, page, bookmark
//                 AcceptedBlockTypes = new List<string> { "text", "header", "sub_header", "sub_sub_header", "bulleted_list", "image", "quote", "column_list", "column", "callout" },
//                 TransformQuote = (data, block) =>
//                 {
//                     sb.Append("<div class=\"notion-quote-block\">").AppendText(data).AppendLine("</div>");
//                     return true;
//                 },
//                 TransformImage = (data, block) =>
//                 {
//                     if (data != null)
//                         sb.Append("<div class=\"notion-image-block\">").AppendImage(data, block.Id).AppendLine("</div>");
//
//                     return true;
//                 },
//                 TransformColumnList = (data, block) =>
//                 {
//                     //Start of notion-column_list-block
//                     sb.Append("<div class=\"notion-column_list-block\"><div style=\"display: flex\">");
//                     var totalColumns = data.Columns.Count;
//
//                     return (true,
//                         StartColumn: (columnIndex, column) =>
//                         {
//                             sb.Append(FormattableString.Invariant($"<div class=\"notion-column\" style=\"width: calc((100% - {46*(totalColumns-1)}px) * {column.Ratio});\">"));
//                             return () => sb.Append("</div>");
//                         },
//                         TransformColumnSeparator: columnIndex => sb.Append("<div class=\"notion-column-separator\"><div class=\"notion-column-separator-line\"></div></div>"),
//                         EndColumnList: () => sb.Append("</div></div>"));
//                 },
//                 TransformCallout = (data, block) =>
//                 {
//                     sb.AppendLine(@$"<div class=""notion-callout-block notion-block-color-{data.Format.BlockColor}"">");
//
//                     if (Uri.TryCreate(data.Format.PageIcon, UriKind.Absolute, out var iconUrl))
//                         sb.AppendLine(@$"<img class=""notion-icon"" src=""{iconUrl}"" />");
//                     else
//                         sb.AppendLine(@$"<img class=""notion-emoji"" src=""{GetTwitterEmojiUrl(data.Format.PageIcon)}"" />");
//                     
//                     transformOptions.TransformText(data.Text, block);
//                     sb.AppendLine("</div>");
//                     return true;
//                 },



        // public static StringBuilder AppendImage(this StringBuilder sb, BlockImageData data, Guid blockId)
        // {
        //     if (data != null)
        //     {
        //         var imageUrl = $"{data.ImageUrl}?table=block&id={blockId:D}&cache=v2";
        //         if (data.Format != null)
        //             sb.Append("<img style=\"width:").Append(data.Format.BlockWidth).Append("px\" src=\"").Append(imageUrl).Append("\"/>");
        //         else
        //             sb.Append("<img src=\"").Append(imageUrl).Append("\"/>");
        //
        //         if (!String.IsNullOrWhiteSpace(data.Caption))
        //             sb.Append("<div class=\"notion-text-block notion-image-caption\">").Append(data.Caption).Append("</div>");
        //     }
        //
        //     return sb;
        // }
        //
        // public static StringBuilder? GetTwitterEmojiUrl(this string emojiString)
        // {
        //     var enc = new UTF32Encoding(true, false);
        //     var bytes = enc.GetBytes(emojiString);
        //
        //     var sbCodepointEmoji = new StringBuilder();
        //     for (var i = 0; i < bytes.Length; i += 4)
        //     {
        //         var value = bytes[i]<<24 | bytes[i + 1]<<16 | bytes[i + 2]<<8 | bytes[i + 3];
        //         if(value == 0xFE0E || value == 0xFE0F)
        //             continue;
        //         sbCodepointEmoji.Append($"{value:x}-");
        //     }
        //
        //     if (sbCodepointEmoji.Length > 0 && sbCodepointEmoji[^1] == '-')
        //         sbCodepointEmoji.Remove(sbCodepointEmoji.Length - 1, 1);
        //
        //     if (sbCodepointEmoji.Length == 0)
        //         return null;
        //
        //     sbCodepointEmoji.Insert(0, "//cdn.jsdelivr.net/gh/twitter/twemoji/assets/svg/");
        //     sbCodepointEmoji.Append(".svg");
        //     return sbCodepointEmoji;
        // }
}
