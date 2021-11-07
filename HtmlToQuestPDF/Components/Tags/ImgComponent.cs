using HtmlAgilityPack;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace Awrosoft.Utilities.HtmlToQuestPDF.Components.Tags
{
    internal class ImgComponent : BaseHTMLComponent
    {
        private readonly GetImgBySrc getImgBySrc;

        public ImgComponent(HtmlNode node, HTMLComponentsArgs args) : base(node, args)
        {
            this.getImgBySrc = args.GetImgBySrc;
        }

        protected override void ComposeSingle(IContainer container)
        {
            var src = node.GetAttributeValue("src", "");
            var img = getImgBySrc(src) ?? Placeholders.Image(200, 100);
    
            // Try to get width and height attributes
            var hasWidth = int.TryParse(node.GetAttributeValue("width", ""), out var width);
            var hasHeight = int.TryParse(node.GetAttributeValue("height", ""), out var height);
    
            // Check for style attribute with width or height
            var style = node.GetAttributeValue("style", "");
            if (!hasWidth && style.Contains("width"))
            {
                var widthMatch = System.Text.RegularExpressions.Regex.Match(style, @"width\s*:\s*(\d+)px");
                if (widthMatch.Success && int.TryParse(widthMatch.Groups[1].Value, out var styleWidth))
                {
                    width = styleWidth;
                    hasWidth = true;
                }
            }
    
            if (!hasHeight && style.Contains("height"))
            {
                var heightMatch = System.Text.RegularExpressions.Regex.Match(style, @"height\s*:\s*(\d+)px");
                if (heightMatch.Success && int.TryParse(heightMatch.Groups[1].Value, out var styleHeight))
                {
                    height = styleHeight;
                    hasHeight = true;
                }
            }
    
            // Configure image with dimensions if provided
            var imageContainer = container.AlignCenter();
    
            if (hasWidth && hasHeight)
            {
                // Apply both constraints separately
                imageContainer = imageContainer.Width(width).Height(height);
                imageContainer.Image(img).FitUnproportionally();
            }
            else if (hasWidth)
            {
                imageContainer.Width(width).Image(img).FitWidth();
            }
            else if (hasHeight)
            {
                imageContainer.Height(height).Image(img).FitHeight();
            }
            else
            {
                imageContainer.Image(img).FitArea();
            }
        }
    }
}