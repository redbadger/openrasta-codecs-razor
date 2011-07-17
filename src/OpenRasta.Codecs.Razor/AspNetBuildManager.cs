namespace OpenRasta.Codecs.Razor
{
    using System;
    using System.Web.Compilation;

    public class AspNetBuildManager : IBuildManager
    {
        public Type GetCompiledType(string path)
        {
            return BuildManager.GetCompiledType(path);
        }
    }
}