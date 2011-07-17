namespace OpenRasta.Codecs.Razor
{
    using System.Globalization;
    using System.Web.Razor.Parser;
    using System.Web.Razor.Parser.SyntaxTree;
    using System.Web.Razor.Text;

    public class OpenRastaCSharpRazorCodeParser : CSharpCodeParser
    {
        private const string ResourceKeyword = "resource";

        private SourceLocation? endInheritsLocation;

        private bool modelStatementFound;

        public OpenRastaCSharpRazorCodeParser()
        {
            this.RazorKeywords.Add(
                ResourceKeyword, this.WrapSimpleBlockParser(BlockType.Directive, this.ParseResourceStatement));
        }

        protected override bool ParseInheritsStatement(CodeBlockInfo block)
        {
            this.endInheritsLocation = this.CurrentLocation;
            bool result = base.ParseInheritsStatement(block);
            this.CheckForInheritsAndResourceStatements();
            return result;
        }

        private void CheckForInheritsAndResourceStatements()
        {
            if (this.modelStatementFound && this.endInheritsLocation.HasValue)
            {
                string message = string.Format(
                    CultureInfo.CurrentCulture, 
                    "The 'inherits' keyword is not allowed when a '{0}' keyword is used.", 
                    ResourceKeyword);
                this.OnError(this.endInheritsLocation.Value, message);
            }
        }

        private bool ParseResourceStatement(CodeBlockInfo block)
        {
            this.End(MetaCodeSpan.Create);

            SourceLocation endModelLocation = this.CurrentLocation;
            if (this.modelStatementFound)
            {
                string message = string.Format(
                    CultureInfo.CurrentCulture, "Only one '{0}' statement is allowed in a file.", ResourceKeyword);
                this.OnError(endModelLocation, message);
            }

            this.modelStatementFound = true;

            // Accept Whitespace up to the new line or non-whitespace character
            this.Context.AcceptWhiteSpace(false);

            string typeName = null;
            if (ParserHelpers.IsIdentifierStart(this.CurrentCharacter))
            {
                using (this.Context.StartTemporaryBuffer())
                {
                    // Accept a dotted-identifier, but allow <>
                    this.AcceptTypeName();
                    typeName = this.Context.ContentBuffer.ToString();
                    this.Context.AcceptTemporaryBuffer();
                }
            }
            else
            {
                string message = string.Format(
                    CultureInfo.CurrentCulture, 
                    "The '{0}' keyword must be followed by a type name on the same line.", 
                    ResourceKeyword);
                this.OnError(endModelLocation, message);
            }

            this.CheckForInheritsAndResourceStatements();
            this.End(ResourceSpan.Create(this.Context, typeName));
            return false;
        }
    }
}