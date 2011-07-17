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
            var context = new InMemoryCommunicationContext
                {
                    Request =
                        new InMemoryRequest
                            {
                               HttpMethod = "GET", Uri = resource, Entity = new HttpEntity { ContentLength = 0 } 
                            }
                };
            context.Request.Headers["Accept"] = MediaType.XhtmlFragment.ToString();
            var textWriterProvider = anchor.AmbientWriter as ISupportsTextWriter;

            StringBuilder inMemoryRendering = null;

            if (textWriterProvider != null && textWriterProvider.TextWriter != null)
            {
                var textWriterEnabledEntity = new TextWriterEnabledEntity(textWriterProvider.TextWriter);
                context.Response = new InMemoryResponse { Entity = textWriterEnabledEntity };
            }
            else
            {
                inMemoryRendering = new StringBuilder();
                var writer = new StringWriter(inMemoryRendering);
                context.Response = new InMemoryResponse { Entity = new TextWriterEnabledEntity(writer) };
            }

            anchor.Resolver.Resolve<IPipeline>().Run(context);

            if (context.Response.Entity.Stream.Length > 0)
            {
                context.Response.Entity.Stream.Position = 0;
                Encoding destinationEncoding =
                    Encoding.GetEncoding(context.Response.Entity.ContentType.CharSet ?? "UTF8");

                var reader = new StreamReader(context.Response.Entity.Stream, destinationEncoding);
                anchor.AmbientWriter.WriteUnencodedString(reader.ReadToEnd());
            }
            else if (inMemoryRendering != null && inMemoryRendering.Length > 0)
            {
                anchor.AmbientWriter.WriteUnencodedString(inMemoryRendering.ToString());
            }
        }
    }
}