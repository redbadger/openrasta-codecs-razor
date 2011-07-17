namespace OpenRasta.Codecs.Razor
{
    using System;
    using System.CodeDom;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Razor;
    using System.Web.Razor.Generator;
    using System.Web.Razor.Parser;
    using System.Web.WebPages;

    public sealed class OpenRastaRazorHost : RazorEngineHost
    {
        internal const string ApplicationInstancePropertyName = "ApplicationInstance";

        internal const string ContextPropertyName = "Context";

        internal const string DefineSectionMethodName = "DefineSection";

        internal const string WebDefaultNamespace = "ASP";

        internal const string WriteLiteralToMethodName = "WriteLiteralTo";

        internal const string WriteToMethodName = "WriteTo";

        internal static readonly string TemplateTypeName = typeof(HelperResult).FullName;

        private static readonly ConcurrentDictionary<string, object> ImportedNamespaces =
            new ConcurrentDictionary<string, object>();

        public OpenRastaRazorHost(RazorCodeLanguage codeLanguage)
        {
            this.NamespaceImports.Add("System");
            this.NamespaceImports.Add("System.Collections.Generic");
            this.NamespaceImports.Add("System.IO");
            this.NamespaceImports.Add("System.Linq");
            this.NamespaceImports.Add("System.Net");
            this.NamespaceImports.Add("System.Web");

            // NamespaceImports.Add("System.Web.Helpers");
            this.NamespaceImports.Add("System.Web.Security");
            this.NamespaceImports.Add("System.Web.UI");
            this.NamespaceImports.Add("System.Web.WebPages");

            this.DefaultNamespace = WebDefaultNamespace;
            this.GeneratedClassContext = new GeneratedClassContext(
                GeneratedClassContext.DefaultExecuteMethodName, 
                GeneratedClassContext.DefaultWriteMethodName, 
                GeneratedClassContext.DefaultWriteLiteralMethodName, 
                WriteToMethodName, 
                WriteLiteralToMethodName, 
                TemplateTypeName, 
                DefineSectionMethodName);
            this.DefaultBaseClass = typeof(RazorViewBase<>).AssemblyQualifiedName;
            this.DefaultDebugCompilation = true;
            this.CodeLanguage = codeLanguage;
        }

        public bool DefaultDebugCompilation { get; set; }

        public static void AddGlobalImport(string ns)
        {
            if (string.IsNullOrEmpty(ns))
            {
                throw new ArgumentException("Argument cannot be null or an empty string.", "ns");
            }

            ImportedNamespaces.TryAdd(ns, null);
        }

        public static IEnumerable<string> GetGlobalImports()
        {
            return ImportedNamespaces.ToArray().Select(pair => pair.Key);
        }

        public override MarkupParser CreateMarkupParser()
        {
            return new HtmlMarkupParser();
        }

        public override RazorCodeGenerator DecorateCodeGenerator(RazorCodeGenerator incomingCodeGenerator)
        {
            if (incomingCodeGenerator is CSharpRazorCodeGenerator)
            {
                return new OpenRastaCSharpRazorCodeGenerator(
                    incomingCodeGenerator.ClassName, 
                    incomingCodeGenerator.RootNamespaceName, 
                    incomingCodeGenerator.SourceFileName, 
                    incomingCodeGenerator.Host);
            }

            if (incomingCodeGenerator is VBRazorCodeGenerator)
            {
                throw new InvalidOperationException("VB not supported yet.");
            }

            return base.DecorateCodeGenerator(incomingCodeGenerator);
        }

        public override ParserBase DecorateCodeParser(ParserBase incomingCodeParser)
        {
            if (incomingCodeParser is CSharpCodeParser)
            {
                return new OpenRastaCSharpRazorCodeParser();
            }

            if (incomingCodeParser is VBCodeParser)
            {
                throw new InvalidOperationException("VB not supported yet.");
            }

            return base.DecorateCodeParser(incomingCodeParser);
        }

        public override void PostProcessGeneratedCode(
            CodeCompileUnit codeCompileUnit, 
            CodeNamespace generatedNamespace, 
            CodeTypeDeclaration generatedClass, 
            CodeMemberMethod executeMethod)
        {
            base.PostProcessGeneratedCode(codeCompileUnit, generatedNamespace, generatedClass, executeMethod);
            generatedNamespace.Imports.AddRange(GetGlobalImports().Select(s => new CodeNamespaceImport(s)).ToArray());
        }
    }
}