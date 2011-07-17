namespace OpenRasta.Codecs.Razor.ConsoleTestApp
{
    using OpenRasta.Configuration;
    using OpenRasta.DI;
    using OpenRasta.Hosting.HttpListener;

    public class HttpListenerHostWithConfiguration : HttpListenerHost
    {
        public IConfigurationSource Configuration { get; set; }

        public override bool ConfigureRootDependencies(IDependencyResolver resolver)
        {
            bool result = base.ConfigureRootDependencies(resolver);
            if (result && this.Configuration != null)
            {
                resolver.AddDependencyInstance<IConfigurationSource>(this.Configuration);
            }

            return result;
        }
    }
}