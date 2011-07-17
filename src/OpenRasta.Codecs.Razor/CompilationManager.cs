namespace OpenRasta.Codecs.Razor
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.WebPages;

    using OpenRasta.Web;

    public class CompilationManager
    {
        private static readonly CodeDomProvider Compiler = CreateCompiler();

        private static readonly object Lock = new object();

        private static Dictionary<string, Type> compiledTypes = new Dictionary<string, Type>();

        public static Type GetCompiledType(string key, Func<CompilationData> compilationDataGenerator)
        {
            Type existing;
            if (compiledTypes.TryGetValue(key, out existing))
            {
                return existing;
            }

            lock (Lock)
            {
                Type newlyCompiledType = CompileType(compilationDataGenerator);
                var newCompiledTypes = new Dictionary<string, Type>(compiledTypes);
                newCompiledTypes[key] = newlyCompiledType;
                compiledTypes = newCompiledTypes;
                return newlyCompiledType;
            }
        }

        private static Type CompileType(Func<CompilationData> compilationDataGenerator)
        {
            CompilationData compilationData = compilationDataGenerator();
            CodeCompileUnit code = compilationData.Code;
            CompilerResults compiled =
                Compiler.CompileAssemblyFromDom(CreateCompilerParameters(compilationData.AdditionalAssemblies), code);

            if (compiled.Errors.HasErrors)
            {
                var sourceCode = new StringBuilder();
                Compiler.GenerateCodeFromCompileUnit(code, new StringWriter(sourceCode), new CodeGeneratorOptions());
                throw new HttpCompileException(compiled, sourceCode.ToString());
            }

            return compiled.CompiledAssembly.GetTypes().First();
        }

        private static CodeDomProvider CreateCompiler()
        {
            var options = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };
            CodeDomProvider compiler = CodeDomProvider.CreateProvider("C#", options);
            return compiler;
        }

        private static CompilerParameters CreateCompilerParameters(IEnumerable<string> additionalAssemblies)
        {
            var referencedAssemblies = new List<string>(additionalAssemblies)
                {
                    "System.dll", 
                    "System.Core.dll", 
                    "System.Web.dll", 
                    "System.Data.dll", 
                    "System.Web.Extensions.dll", 
                    "Microsoft.CSharp.dll", 
                    typeof(IRequest).Assembly.Location, 
                    typeof(HelperResult).Assembly.Location, 
                    typeof(StandAloneBuildManager).Assembly.Location
                };

            var parameters = new CompilerParameters();
            parameters.ReferencedAssemblies.AddRange(referencedAssemblies.ToArray());
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;

            return parameters;
        }
    }
}