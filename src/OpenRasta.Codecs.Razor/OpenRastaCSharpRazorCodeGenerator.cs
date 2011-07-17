namespace OpenRasta.Codecs.Razor
{
    using System.CodeDom;
    using System.Web.Razor;
    using System.Web.Razor.Generator;
    using System.Web.Razor.Parser.SyntaxTree;

    public class OpenRastaCSharpRazorCodeGenerator : CSharpRazorCodeGenerator
    {
        public OpenRastaCSharpRazorCodeGenerator(
            string className, string rootNamespaceName, string sourceFileName, RazorEngineHost host)
            : base(className, rootNamespaceName, sourceFileName, host)
        {
        }

        protected override bool TryVisitSpecialSpan(Span span)
        {
            return TryVisit<ResourceSpan>(span, this.VisitResourceSpan);
        }

        private void VisitResourceSpan(ResourceSpan span)
        {
            string modelName = span.ResourceTypeName;
            var baseType = new CodeTypeReference(this.Host.DefaultBaseClass, new CodeTypeReference(modelName));

            this.GeneratedClass.BaseTypes.Clear();
            this.GeneratedClass.BaseTypes.Add(baseType);

            if (this.DesignTimeMode)
            {
                this.WriteHelperVariable(span.Content, "__modelHelper");
            }
        }
    }
}