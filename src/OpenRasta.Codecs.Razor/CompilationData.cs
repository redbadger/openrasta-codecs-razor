namespace OpenRasta.Codecs.Razor
{
    using System.CodeDom;
    using System.Collections.Generic;

    public class CompilationData
    {
        private readonly IEnumerable<string> additionalAssemblies;

        private readonly CodeCompileUnit code;

        public CompilationData(IEnumerable<string> additionalAssemblies, CodeCompileUnit code)
        {
            this.additionalAssemblies = additionalAssemblies;
            this.code = code;
        }

        public IEnumerable<string> AdditionalAssemblies
        {
            get
            {
                return this.additionalAssemblies;
            }
        }

        public CodeCompileUnit Code
        {
            get
            {
                return this.code;
            }
        }
    }
}