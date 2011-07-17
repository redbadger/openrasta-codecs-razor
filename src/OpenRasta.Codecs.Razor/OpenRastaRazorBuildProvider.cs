namespace OpenRasta.Codecs.Razor
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Security;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Hosting;
    using System.Web.Razor;
    using System.Web.Razor.Parser;
    using System.Web.Razor.Parser.SyntaxTree;

    [BuildProviderAppliesTo(BuildProviderAppliesTo.Web | BuildProviderAppliesTo.Code)]
    public class OpenRastaRazorBuildProvider : BuildProvider
    {
        private static bool? isFullTrust;

        private CodeCompileUnit generatedCode;

        private OpenRastaRazorHost host;

        private string physicalPath;

        public override CompilerType CodeCompilerType
        {
            get
            {
                this.EnsureGeneratedCode();
                CompilerType compilerType = this.GetDefaultCompilerTypeForLanguage(this.Host.CodeLanguage.LanguageName);
                if (isFullTrust != false && this.Host.DefaultDebugCompilation)
                {
                    try
                    {
                        SetIncludeDebugInfoFlag(compilerType);
                        isFullTrust = true;
                    }
                    catch (SecurityException)
                    {
                        isFullTrust = false;
                    }
                }

                return compilerType;
            }
        }

        public string PhysicalPath
        {
            get
            {
                this.MapPhysicalPath();
                return this.physicalPath;
            }

            set
            {
                this.physicalPath = value;
            }
        }

        private CodeCompileUnit GeneratedCode
        {
            get
            {
                this.EnsureGeneratedCode();
                return this.generatedCode;
            }
        }

        private OpenRastaRazorHost Host
        {
            get
            {
                return this.host ?? (this.host = this.CreateHost());
            }
        }

        public override void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
            assemblyBuilder.AddCodeCompileUnit(this, this.GeneratedCode);
        }

        public override Type GetGeneratedType(CompilerResults results)
        {
            return
                results.CompiledAssembly.GetType(
                    string.Format(
                        CultureInfo.CurrentCulture, "{0}.{1}", this.Host.DefaultNamespace, this.GetClassName()));
        }

        private static HttpParseException CreateExceptionFromParserError(RazorError error, string virtualPath)
        {
            return new HttpParseException(
                error.Message + Environment.NewLine, null, virtualPath, null, error.Location.LineIndex + 1);
        }

        private static RazorCodeLanguage DetermineCodeLanguage(string fileName)
        {
            string extension = Path.GetExtension(fileName);

            // Use an if rather than else-if just in case Path.GetExtension returns null for some reason
            if (string.IsNullOrEmpty(extension))
            {
                return null;
            }

            if (extension[0] == '.')
            {
                extension = extension.Substring(1); // Trim off the dot
            }

            // Look up the language
            // At the moment this only deals with code languages: cs, vb, etc., but in theory we could have MarkupLanguageServices which allow for
            // interesting combinations like: vbcss, csxml, etc.
            RazorCodeLanguage language = RazorCodeLanguage.GetLanguageByExtension(extension);
            return language;
        }

        private static void SetIncludeDebugInfoFlag(CompilerType compilerType)
        {
            compilerType.CompilerParameters.IncludeDebugInformation = true;
        }

        private OpenRastaRazorHost CreateHost()
        {
            return OpenRastaRazorHostFactory.CreateHost(this.GetCodeLanguage());
        }

        private void EnsureGeneratedCode()
        {
            if (this.generatedCode == null)
            {
                var engine = new RazorTemplateEngine(this.Host);
                GeneratorResults results;
                using (TextReader reader = this.OpenReader())
                {
                    results = engine.GenerateCode(reader, this.GetClassName(), this.Host.DefaultNamespace, null);
                }

                if (!results.Success)
                {
                    throw CreateExceptionFromParserError(results.ParserErrors.Last(), this.VirtualPath);
                }

                this.generatedCode = results.GeneratedCode;
            }
        }

        private string GetClassName()
        {
            return ParserHelpers.SanitizeClassName(Path.GetFileName(this.VirtualPath));
        }

        private RazorCodeLanguage GetCodeLanguage()
        {
            RazorCodeLanguage language = DetermineCodeLanguage(this.VirtualPath);
            if (language == null && !string.IsNullOrEmpty(this.PhysicalPath))
            {
                language = DetermineCodeLanguage(this.PhysicalPath);
            }

            if (language == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture, "Could not determine the code language for '{0}'", this.VirtualPath));
            }

            return language;
        }

        private void MapPhysicalPath()
        {
            if (this.physicalPath == null && HostingEnvironment.IsHosted)
            {
                string path = HostingEnvironment.MapPath(this.VirtualPath);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    this.physicalPath = path;
                }
            }
        }
    }
}