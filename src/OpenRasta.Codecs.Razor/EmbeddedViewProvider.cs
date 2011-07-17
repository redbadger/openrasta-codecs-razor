namespace OpenRasta.Codecs.Razor
{
    using System.IO;
    using System.Reflection;

    public class EmbeddedViewProvider : IViewProvider
    {
        private readonly Assembly assembly;

        private readonly string baseNamespace;

        public EmbeddedViewProvider(Assembly assembly, string baseNamespace)
        {
            this.assembly = assembly;
            this.baseNamespace = baseNamespace;
        }

        public ViewDefinition GetViewDefinition(string path)
        {
            Stream stream = this.assembly.GetManifestResourceStream(string.Join(".", this.baseNamespace, path));
            if (stream == null)
            {
                return null;
            }

            var reader = new StreamReader(stream);
            return new ViewDefinition(path, reader, this.assembly);
        }
    }
}