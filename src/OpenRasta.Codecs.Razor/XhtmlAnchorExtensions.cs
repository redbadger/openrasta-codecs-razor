namespace OpenRasta.Codecs.Razor
{
    using System;
    using System.IO;
    using System.Text;

    using OpenRasta.DI;
    using OpenRasta.Hosting.InMemory;
    using OpenRasta.Pipeline;
    using OpenRasta.Pipeline.InMemory;
    using OpenRasta.Web;
    using OpenRasta.Web.Markup;

    public static class XhtmlAnchorExtensions
    {
        public static void RenderResource(this IXhtmlAnchor anchor, Uri resource)
        {
            IDependencyResolver resolver = anchor.Resolver;
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

            var textWriterProvider = anchor.AmbientWriter as ISupportsTextWriter;

            StringBuilder inMemoryRendering = null;

            if (textWriterProvider != null && textWriterProvider.TextWriter != null)
            {
                var textWriterEnabledEntity = new TextWriterEnabledEntity(textWriterProvider.TextWriter);
                newContext.Response = new InMemoryResponse { Entity = textWriterEnabledEntity };
            }
            else
            {
                inMemoryRendering = new StringBuilder();
                var writer = new StringWriter(inMemoryRendering);
                newContext.Response = new InMemoryResponse { Entity = new TextWriterEnabledEntity(writer) };
            }

            // Push new context
            resolver.AddDependencyInstance<ICommunicationContext>(newContext, DependencyLifetime.PerRequest);
            resolver.AddDependencyInstance<IRequest>(newContext.Request, DependencyLifetime.PerRequest);
            resolver.AddDependencyInstance<IResponse>(newContext.Response, DependencyLifetime.PerRequest);

            resolver.Resolve<IPipeline>().Run(newContext);

            // Pop old context
            resolver.AddDependencyInstance<ICommunicationContext>(oldContext, DependencyLifetime.PerRequest);
            resolver.AddDependencyInstance<IRequest>(oldContext.Request, DependencyLifetime.PerRequest);
            resolver.AddDependencyInstance<IResponse>(oldContext.Response, DependencyLifetime.PerRequest);

            if (newContext.Response.Entity.Stream.Length > 0)
            {
                newContext.Response.Entity.Stream.Position = 0;
                Encoding destinationEncoding =
                    Encoding.GetEncoding(newContext.Response.Entity.ContentType.CharSet ?? "UTF8");

                var reader = new StreamReader(newContext.Response.Entity.Stream, destinationEncoding);
                anchor.AmbientWriter.WriteUnencodedString(reader.ReadToEnd());
            }
            else if (inMemoryRendering != null && inMemoryRendering.Length > 0)
            {
                anchor.AmbientWriter.WriteUnencodedString(inMemoryRendering.ToString());
            }
        }
    }
}