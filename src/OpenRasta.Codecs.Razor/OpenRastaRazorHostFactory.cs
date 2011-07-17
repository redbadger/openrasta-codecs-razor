namespace OpenRasta.Codecs.Razor
{
    using System.Web.Razor;

    public class OpenRastaRazorHostFactory
    {
        public static OpenRastaRazorHost CreateHost(RazorCodeLanguage codeLanguage)
        {
            return new OpenRastaRazorHost(codeLanguage);
        }
    }
}