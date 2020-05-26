using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NotionSharp.Lib.ApiV3.Model;

namespace NotionSharp
{
    public class TransformOptions
    {
        /// <summary>
        /// block types that are selected
        /// </summary>
        /// <remarks>
        /// null = all types accepted
        /// </remarks>
        public IList<string> AcceptedBlockTypes { get; set; }

        /// <summary>
        /// Maximum blocks to transform (from AcceptedBlockTypes)
        /// </summary>
        /// <remarks>
        /// 0 = all blocks
        /// </remarks>
        public int MaxBlocks { get; set; }

        /// <summary>
        /// Optional
        /// </summary>
        public Func<BlockTextData, Block, bool> TransformText { get; set; }
        /// <summary>
        /// Optional. H1.
        /// </summary>
        public Func<BlockTextData, Block, bool> TransformHeader { get; set; }
        /// <summary>
        /// Optional. H2.
        /// </summary>
        public Func<BlockTextData, Block, bool> TransformSubHeader { get; set; }
        /// <summary>
        /// Optional. UL+LI.
        /// </summary>
        public Func<BlockTextData, Block, bool> TransformBulletedList { get; set; }

        /// <summary>
        /// Optional
        /// </summary>
        /// <remarks>
        /// BlockImageData can be null
        /// </remarks>
        public Func<BlockImageData, Block, bool> TransformImage { get; set; }

        /// <summary>
        /// Optional
        /// </summary>
        public Func<Block, bool> TransformOther { get; set; }

        /// <summary>
        /// If a block in the selected page is missing (ie: page content is not fully loaded), throw an exception
        /// </summary>
        public bool ThrowIfBlockMissing { get; set; } = true;

        public bool ThrowIfCantDecodeTextData { get; set; } = true;
    }

    public class BlockTextData
    {
        public IList<BlockTextPart> Lines { get; set; }
    }

    public class BlockTextPart
    {
        public string Text { get; set; }
        public bool HasProperty => HyperlinkUrl != null || HasStyle;
        public bool HasStyle => IsItalic || IsBold || HtmlColor != null;

        public string HyperlinkUrl { get; set; }
        public bool IsItalic { get; set; }
        public bool IsBold { get; set; }
        public string HtmlColor { get; set; }
    }

    public class BlockImageData
    {
        /// <summary>
        /// Can not be null
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Can be null
        /// </summary>
        public string ImagePrivateUrl { get; set; }

        /// <summary>
        /// Can be null
        /// </summary>
        public BlockImageFormat Format { get; set; }
    }

    public static class BlockExtensions
    {
        public static void Transform(this RecordMap recordMap, TransformOptions transformOptions,  Guid pageId = default)
        {
            var pageBlock = pageId != default ?
                recordMap.Block[pageId]
                : recordMap.Block.First(b => b.Value.Type == "page").Value;

            var blocks = from itemId in pageBlock.Content
                         let block = !transformOptions.ThrowIfBlockMissing && !recordMap.Block.ContainsKey(itemId) ? null : recordMap.Block[itemId]
                         where block != null && (transformOptions == null || transformOptions.AcceptedBlockTypes.Contains(block.Type))
                         select block;

            if (transformOptions.MaxBlocks > 0)
                blocks = blocks.Take(transformOptions.MaxBlocks);

            foreach (var block in blocks)
            {
                var okToContinue = block.Type switch
                {
                    "text" => transformOptions.TransformText?.Invoke(block.ToTextData(transformOptions.ThrowIfCantDecodeTextData), block),
                    "header" => transformOptions.TransformHeader?.Invoke(block.ToTextData(transformOptions.ThrowIfCantDecodeTextData), block),
                    "sub_header" => transformOptions.TransformSubHeader?.Invoke(block.ToTextData(transformOptions.ThrowIfCantDecodeTextData), block),
                    "bulleted_list" => transformOptions.TransformBulletedList?.Invoke(block.ToTextData(transformOptions.ThrowIfCantDecodeTextData), block),
                    "image" => transformOptions.TransformImage?.Invoke(block.ToImageData(), block),
                    _ => transformOptions.TransformOther?.Invoke(block)
                };

                if (!(okToContinue != false))
                    break;
            }
        }

        internal static BlockImageData ToImageData(this Block imageBlock)
        {
            if (imageBlock.Type != "image")
                throw new ArgumentException($"textBlock.Type must be image, currently is {imageBlock.Type}", nameof(imageBlock));

            if (imageBlock.Properties != null)
            {
                var data = new BlockImageData();

                if (imageBlock.Properties.ContainsKey("format"))
                {
                    data.Format = imageBlock.Properties["format"].ToObject<BlockImageFormat>();
                    data.ImageUrl = data.Format.DisplaySource;
                }
                else if (imageBlock.Properties.ContainsKey("source"))
                {
                    data.ImagePrivateUrl = (string)imageBlock.Properties["source"][0][0];
                    data.ImageUrl = $"https://www.notion.so/image/{Uri.EscapeDataString(data.ImagePrivateUrl)}";
                }
                else
                    return null;

                return data;
            }

            return null;
        }

        internal static BlockTextData ToTextData(this Block textBlock, bool throwIfCantDecodeTextData)
        {
            if (textBlock.Type != "text" && textBlock.Type != "sub_header")
                throw new ArgumentException($"textBlock.Type must be text or sub_header, currently is {textBlock.Type}", nameof(textBlock));

            var data = new BlockTextData { Lines = new List<BlockTextPart>() };

            if (textBlock.Properties != null && textBlock.Properties.ContainsKey("title"))
            {
                var textParts = ((JArray)textBlock.Properties["title"]).Cast<JArray>();

                foreach (var textPart in textParts)
                    data.Lines.Add(ToTextData(textPart, throwIfCantDecodeTextData));
            }

            return data;
        }

        static BlockTextPart ToTextData(JArray textPart, bool throwIfCantDecodeTextData)
        {
            var blockTextPart = new BlockTextPart { Text = (string)textPart[0] };

            if (textPart.Count == 1)
                return blockTextPart;

            if (textPart.Count == 2)
            {
                if (textPart[1] is JArray subParts)
                {
                    foreach (var subPart in subParts)
                    {
                        if (subPart is JArray outerTag && outerTag.All(t => !(t is JArray)))
                        {
                            var outerTagValue = (string)outerTag[0];
                            if (outerTagValue == "a" && outerTag.Count == 2)
                                blockTextPart.HyperlinkUrl = (string)outerTag[1];
                            else if (outerTagValue == "h" && outerTag.Count == 2)
                                blockTextPart.HtmlColor = (string)outerTag[1];
                            else if (outerTag.Count == 1 && (outerTagValue == "i" || outerTagValue == "b"))
                            {
                                //bold, italic
                                if(outerTagValue == "b")
                                    blockTextPart.IsBold = true;
                                else if (outerTagValue == "i")
                                    blockTextPart.IsItalic = true;
                            }
                            else if(throwIfCantDecodeTextData)
                                throw new NotSupportedException($"unknown outer tag {outerTag[0]} with counts:{outerTag.Count}");
                        }
                        else if (throwIfCantDecodeTextData)
                            throw new NotSupportedException($"unknown subParts {subParts}");
                    }
                }
                else if (throwIfCantDecodeTextData)
                    throw new NotSupportedException($"unknown subParts structure {textPart[1]}");
            }
            else if (throwIfCantDecodeTextData)
                throw new NotSupportedException($"this decoder supports only 1 or 2 textParts. This block has {textPart.Count} parts. textPart: {textPart}");

            return blockTextPart;
        }
    }
}
