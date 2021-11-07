using Awrosoft.Utilities.HtmlToQuestPDF.Extensions;
using HtmlAgilityPack;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace Awrosoft.Utilities.HtmlToQuestPDF.Components.Tags
{
    internal class ListComponent : BaseHTMLComponent
    {
        public ListComponent(HtmlNode node, HTMLComponentsArgs args) : base(node, args)
        {
        }

        protected override IContainer ApplyStyles(IContainer container)
        {
            var first = IsFirstList(node);
            return base.ApplyStyles(container).Element(e => first ? e.PaddingVertical(args.ListVerticalPadding) : e);
        }

        private bool IsFirstList(HtmlNode node)
        {
            if (node.ParentNode == null) return true;
            return !node.ParentNode.IsList() && IsFirstList(node.ParentNode);
        }
    }
}