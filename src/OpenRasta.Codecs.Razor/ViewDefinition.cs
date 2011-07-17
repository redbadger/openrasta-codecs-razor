namespace OpenRasta.Codecs.Razor
{
    using System.IO;
    using System.Reflection;

    public class ViewDefinition
    {
        private readonly TextReader contents;

        private readonly string fileName;

        private readonly Assembly viewAssembly;

        public ViewDefinition(string fileName, TextReader contents, Assembly viewAssembly)
        {
            this.fileName = fileName;
            this.viewAssembly = viewAssembly;
            this.contents = contents;
        }

        public TextReader Contents
        {
            get
            {
                return this.contents;
            }
        }

        public string FileName
        {
            get
            {
                return this.fileName;
            }
        }

        public Assembly ViewAssembly
        {
            get
            {
                return this.viewAssembly;
            }
        }
    }
}