namespace OpenRasta.Codecs.Razor.Specs
{
    using System.IO;
    using System.Reflection;

    public class FakeViewProvider : IViewProvider
    {
        private readonly string viewCode;

        public FakeViewProvider(string viewCode)
        {
            this.viewCode = viewCode;
        }

        public ViewDefinition GetViewDefinition(string path)
        {
            return new ViewDefinition(
                "ViewFile.cshtml", new StringReader(this.viewCode), Assembly.GetExecutingAssembly());
        }
    }
}