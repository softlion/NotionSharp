using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotionSharp.ApiClient.Lib.HtmlRendering;

public class HtmlRenderer
{
    /// <summary>
    /// Get an HTML extract of the page
    /// </summary>
    /// <param name="blocks">the page's child blocks</param>
    /// <param name="stopBeforeFirstSubHeader">true to return only all html before the first sub-header</param>
    /// <returns>An HTML string</returns>
    public virtual string GetHtml(List<Block> blocks, bool stopBeforeFirstSubHeader = false)
    {
        var sb = new StringBuilder();

        foreach (var block in blocks)
        {
            if (!Transform(block, sb, stopBeforeFirstSubHeader))
                break;
        }

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
                TransformImage(block.Image, sb);
                break;
            // case BlockTypes.File:
            //     TransformFile(block.File, sb);
            //     break;
            case BlockTypes.Quote:
                TransformQuote(block, sb);
                break;
            case BlockTypes.Callout:
                TransformCallout(block, sb);
                break;
            case BlockTypes.ColumnList:
                TransformColumnList(block, sb);
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

    protected virtual void TransformImage(NotionFile? data, StringBuilder sb)
    {
        var url = data?.External?.Url ?? data?.File?.Url;
        if (url != null)
        {
            sb.Append("<div class=\"notion-image-block\">")
                .Append("<img src=\"").Append(url).Append("\"/>")
                .AppendLine("</div>");
        }
    }

    protected virtual void TransformQuote(Block block, StringBuilder sb)
    {
        var quote = block.Quote!;
        var color = quote.Color.GetColor();
 
        sb.Append("<div class=\"notion-quote-block");
        if(color != null)
            sb.Append($" notion-block-color-{color}>");
        sb.Append("\">");

        Append(quote, sb);
        sb.AppendLine("</div>");
    }
    
    protected virtual void TransformCallout(Block block, StringBuilder sb)
    {
        var callout = block.Callout!;
        var color = callout.Color.GetColor();

        sb.Append("""<div class="notion-callout-block""");
        if(color != null)
            sb.Append($""" notion-block-color-{color}""");
        sb.Append("\">");

        if (callout.Icon is NotionEmoji emoji)
            sb.AppendLine($"""<img class="notion-emoji" src="{emoji.Emoji.GetTwitterEmojiUrl()}" />""");
        else if(callout.Icon is NotionFile file
                && Uri.TryCreate(file.External?.Url ?? file.File?.Url, UriKind.Absolute, out var iconUrl))
            sb.AppendLine($"""<img class="notion-icon" src="{iconUrl}" />""");

        Append(callout.RichText, sb);
        sb.AppendLine("</div>");
    }

    /// <summary>
    /// Manage a column list
    /// </summary>
    /// <remarks>
    /// Children are exclusively columns.
    /// Each column also has children that represent the column's content.
    /// </remarks>
    protected virtual void TransformColumnList(Block block, StringBuilder sb)
    {
        if(!block.HasChildren)
            return;

        var result = TransformColumnListInner(block, sb);

        var columnIndex = 0;
        var totalColumns = block.Children.Count;
        foreach (var columnBlock in block.Children) //type=BlockTypes.Column
        {
            var endColumn = result.StartColumn(columnIndex);
            foreach (var contentBlock in columnBlock.Children)
                Transform(contentBlock, sb, false);
            endColumn();
                
            if (columnIndex < totalColumns - 1)
                result.TransformColumnSeparator(columnIndex);

            columnIndex++;
        }

        result.EndColumnList();
    }


    /// <summary>
    /// Render a column
    /// </summary>
    protected virtual (Func<int, Action> StartColumn, 
                       Action<int> TransformColumnSeparator, 
                       Action EndColumnList) 
                       TransformColumnListInner(Block columnBlock, StringBuilder sb)
    {
        //Start column list
        sb.Append("<div class=\"notion-column_list-block\"><div style=\"display: flex\">");
        var totalColumns = columnBlock.Children.Count;

        //Return actions to render each column
        return (
            //Render a column
            (columnIndex) =>
            {
                //Start this column
                const int ratio = 1; //column.Ratio is not available in public api
                sb.Append(FormattableString.Invariant($"""<div class="notion-column" style="width: calc((100% - {46*(totalColumns-1)}px) * {ratio});">"""));
                //Return an action that renders the end of this column
                return () => sb.Append("</div>");
            },
            //Render the column list separator
            columnIndex => sb.Append("""<div class="notion-column-separator"><div class="notion-column-separator-line"></div></div>"""),
            //Render the column list close tag 
            () => sb.Append("</div></div>")
            );
    }

    protected virtual void TransformBulletedListItem(Block block, StringBuilder sb)
    {
        sb.Append("<ul><li>");
        Append(block.BulletedListItem, sb);
        if (block?.Children != null)
        {
            foreach (var child in block.Children)
                Transform(child, sb, false);
        }
        sb.AppendLine("</li></ul>");
    }

    protected virtual void TransformNumberedListItem(Block block, StringBuilder sb)
    {
        sb.Append("<ol><li>");
        Append(block.NumberedListItem, sb);
        if (block?.Children != null)
        {
            foreach (var child in block.Children)
                Transform(child, sb, false);
        }
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
        Append(block.Heading2?.RichText, sb).AppendLine("</h2>");
    }
    protected virtual void TransformHeading3(Block block, StringBuilder sb)
    {
        sb.Append("<h3>");
        Append(block.Heading3?.RichText, sb).AppendLine("</h3>");
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
        var hasAnnotations = line.Annotation?.HasAnnotation == true;
        var hasLink = !string.IsNullOrWhiteSpace(line.Href);
        var tag = hasLink ? "a" : hasAnnotations ? "span" : null;

        if (tag != null)
        {
            sb.Append("<").Append(tag);

            if (hasLink)
                sb.Append(" href=\"").Append(Uri.EscapeUriString(line.Href)).Append("\"");

            if (hasAnnotations)
            {
                sb.Append(" class=\"");
                if (line.Annotation!.Bold)
                    sb.Append(" notion-bold");
                if (line.Annotation.Italic)
                    sb.Append(" notion-italic");
                if (line.Annotation.Strikethrough)
                    sb.Append(" notion-strikethrough");
                if (line.Annotation.Underline)
                    sb.Append(" notion-underline");
                if (line.Annotation.Color is not null and not NotionColor.Default)
                    sb.Append(" notion-color-").Append(line.Annotation.Color);
                if (line.Annotation.Code)
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

        // var hasLink = text.Link?.Url != null;
        // if (hasLink)
        //     sb.Append("<a href=\"").Append(Uri.EscapeUriString(text.Link.Url)).Append("\">");
        sb.Append(text.Content);
        // if (hasLink)
        //     sb.Append("</a>");

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




