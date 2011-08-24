namespace OpenRasta.Codecs.Razor
{
    using System;
    using System.Web.WebPages;

    using OpenRasta.DI;
    using OpenRasta.Hosting.InMemory;
    using OpenRasta.Pipeline;
    using OpenRasta.Pipeline.InMemory;
    using OpenRasta.Web;

    public static class WebPageBaseExtensions
    {
        public static void RenderResource(this WebPageBase page, Uri resource)
        {
            var resolver = DependencyManager.GetService<IDependencyResolver>();
            var oldContext = resolver.Resolve<ICommunicationContext>();

            var newContext = new InMemoryCommunicationContext
                {
                    Request =
                        new InMemoryRequest
                            {
                                HttpMethod = "GET",
                                Uri = resource,
                                Entity = new HttpEntity { ContentLength = 0 },
                                Headers = oldContext.Request.Headers,
                                NegotiatedCulture = oldContext.Request.NegotiatedCulture
                            },
                    ApplicationBaseUri = oldContext.ApplicationBaseUri,
                    User = oldContext.User
                };
            newContext.Request.Headers["Accept"] = MediaType.XhtmlFragment.ToString();

            var textWriterEnabledEntity = new TextWriterEnabledEntity(page.Output);
            newContext.Response = new InMemoryResponse { Entity = textWriterEnabledEntity };

            // Push context
            resolver.AddDependencyInstance<ICommunicationContext>(newContext, DependencyLifetime.PerRequest);
            resolver.AddDependencyInstance<IRequest>(newContext.Request, DependencyLifetime.PerRequest);
            resolver.AddDependencyInstance<IResponse>(newContext.Response, DependencyLifetime.PerRequest);

            resolver.Resolve<IPipeline>().Run(newContext);

            // Pop context
            resolver.AddDependencyInstance<ICommunicationContext>(oldContext, DependencyLifetime.PerRequest);
            resolver.AddDependencyInstance<IRequest>(oldContext.Request, DependencyLifetime.PerRequest);
            resolver.AddDependencyInstance<IResponse>(oldContext.Response, DependencyLifetime.PerRequest);
        }
    }
}