namespace OpenRasta.Codecs.Razor.ConsoleTestApp.Handlers
{
    using OpenRasta.Codecs.Razor.ConsoleTestApp.Resources;

    public class TestHandler
    {
        public TestResource Get()
        {
            return new TestResource { TestString = "Hello, OpenRasta!" };
        }
    }
}