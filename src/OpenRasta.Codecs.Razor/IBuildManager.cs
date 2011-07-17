namespace OpenRasta.Codecs.Razor
{
    using System;

    public interface IBuildManager
    {
        Type GetCompiledType(string path);
    }
}