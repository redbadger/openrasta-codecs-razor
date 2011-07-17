namespace OpenRasta.Codecs.Razor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web.Hosting;

    using OpenRasta.Collections.Specialized;
    using OpenRasta.DI;
    using OpenRasta.IO;
    using OpenRasta.Web;

    [MediaType("application/xhtml+xml;q=0.9", "xhtml")]
    [MediaType("text/html", "html")]
    [MediaType("application/vnd.openrasta.htmlfragment+xml;q=0.5")]
    [SupportedType(typeof(RazorViewBase))]
    public class RazorCodec : IMediaTypeWriter
    {
        private static readonly string[] DefaultViewNames = new[] { "index", "default", "view", "get" };

        private readonly IBuildManager buildManager;

        private readonly IRequest request;

        private IDictionary<string, string> configuration;

        public RazorCodec(IRequest request)
        {
            this.request = request;
            this.buildManager = CreateBuildManager();
        }

        public object Configuration
        {
            get
            {
                return this.configuration;
            }

            set
            {
                if (value != null)
                {
                    this.configuration = value.ToCaseInvariantDictionary();
                }
            }
        }

        public static string GetViewVPath(
            IDictionary<string, string> codecConfiguration, string[] codecUriParameters, string uriName)
        {
            // if no pages were defined, return 501 not implemented
            if (codecConfiguration == null || codecConfiguration.Count == 0)
            {
                return null;
            }

            // if no codec parameters in the uri, take the default or return null
            if (codecUriParameters == null || codecUriParameters.Length == 0)
            {
                if (uriName != null && codecConfiguration.ContainsKey(uriName))
                {
                    return codecConfiguration[uriName];
                }

                return GetDefaultVPath(codecConfiguration);
            }

            // if there's a codec parameter, take the first one and try to return the view if it exists
            string requestParameter = codecUriParameters[codecUriParameters.Length - 1];
            if (codecConfiguration.Keys.Contains(requestParameter))
            {
                return codecConfiguration[requestParameter];
            }

            // if theres a codec parameter and a uri name that doesn't match it, return teh default
            if (!uriName.IsNullOrEmpty())
            {
                return GetDefaultVPath(codecConfiguration);
            }

            return null;
        }

        public void WriteTo(object entity, IHttpEntity response, string[] codecParameters)
        {
            // The default webforms renderer only associate the last parameter in the codecParameters
            // with a page that has been defined in the rendererParameters.
            var codecParameterList = new List<string>(codecParameters);
            if (!string.IsNullOrEmpty(this.request.UriName))
            {
                codecParameterList.Add(this.request.UriName);
            }

            string templateAddress = GetViewVPath(
                this.configuration, codecParameterList.ToArray(), this.request.UriName);

            Type type = this.buildManager.GetCompiledType(templateAddress);

            var renderTarget = DependencyManager.GetService(type) as RazorViewBase;

            if (renderTarget == null)
            {
                throw new InvalidOperationException("View page doesn't inherit from RazorViewBase");
            }

            renderTarget.SetResource(entity);
            renderTarget.Errors = response.Errors;
            RenderTarget(response, renderTarget);
        }

        private static IBuildManager CreateBuildManager()
        {
            if (HostingEnvironment.IsHosted)
            {
                return new AspNetBuildManager();
            }

            return new StandAloneBuildManager(DependencyManager.GetService<IViewProvider>());
        }

        private static string GetDefaultVPath(IDictionary<string, string> codecConfiguration)
        {
            return (from defaultViewName in DefaultViewNames
                    where codecConfiguration.Keys.Contains(defaultViewName)
                    select codecConfiguration[defaultViewName]).FirstOrDefault();
        }

        private static void RenderTarget(IHttpEntity response, RazorViewBase target)
        {
            Encoding targetEncoding = Encoding.UTF8;
            response.ContentType.CharSet = targetEncoding.HeaderName;
            TextWriter writer = null;
            var isDisposable = target as IDisposable;
            bool ownsWriter = false;
            try
            {
                if (response is ISupportsTextWriter)
                {
                    writer = ((ISupportsTextWriter)response).TextWriter;
                }
                else
                {
                    writer = new DeterministicStreamWriter(response.Stream, targetEncoding, StreamActionOnDispose.None);
                    ownsWriter = true;
                }

                target.Output = writer;
                target.Execute();
            }
            finally
            {
                if (isDisposable != null)
                {
                    isDisposable.Dispose();
                }

                if (ownsWriter)
                {
                    writer.Dispose();
                }
            }
        }
    }
}