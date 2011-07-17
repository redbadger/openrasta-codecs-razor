namespace OpenRasta.Codecs.Razor.TestApp
{
    using OpenRasta.Codecs.Razor.TestApp.Handlers;
    using OpenRasta.Codecs.Razor.TestApp.Resources;
    using OpenRasta.Configuration;

    public class Configuration : IConfigurationSource
    {
        public void Configure()
        {
            using (OpenRastaConfiguration.Manual)
            {                
                ResourceSpace.Has.ResourcesOfType<TestResource>()
                    .AtUri("/home")
                    .And.AtUri("/")
                    .HandledBy<TestHandler>()
                    .RenderedByRazor(new { index = "~/Views/TestView.cshtml" });
            }
        }
    }
}