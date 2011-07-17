namespace OpenRasta.Codecs.Razor.TestApp.Handlers
{
    using OpenRasta.Codecs.Razor.TestApp.Resources;

    public class TestHandler
    {
        public TestResource Get()
        {
            return new TestResource { TestString = "Hello, OpenRasta!" };
        }
    }
}