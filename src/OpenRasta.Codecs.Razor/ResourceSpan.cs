namespace OpenRasta.Codecs.Razor
{
    using System;
    using System.Web.Razor.Parser;
    using System.Web.Razor.Parser.SyntaxTree;
    using System.Web.Razor.Text;

    public class ResourceSpan : CodeSpan
    {
        private readonly string resourceTypeName;

        public ResourceSpan(SourceLocation start, string content, string modelTypeName)
            : base(start, content)
        {
            this.resourceTypeName = modelTypeName;
        }

        public string ResourceTypeName
        {
            get
            {
                return this.resourceTypeName;
            }
        }

        public static new ResourceSpan Create(ParserContext context, string modelTypeName)
        {
            return new ResourceSpan(context.CurrentSpanStart, context.ContentBuffer.ToString(), modelTypeName);
        }

        public override bool Equals(object obj)
        {
            var span = obj as ResourceSpan;
            return span != null && this.Equals(span);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (this.resourceTypeName ?? string.Empty).GetHashCode();
        }

        private bool Equals(ResourceSpan span)
        {
            return base.Equals(span)
                   && string.Equals(this.resourceTypeName, span.resourceTypeName, StringComparison.Ordinal);
        }
    }
}