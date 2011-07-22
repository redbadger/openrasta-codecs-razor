namespace OpenRasta.Codecs.Razor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.WebPages;

    using Microsoft.Web.Infrastructure.DynamicValidationHelper;

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
            renderTarget.VirtualPath = templateAddress;
            this.RenderTarget(response, renderTarget);
        }

        internal void ProcessRequestInternal(HttpContext context, WebPageBase page, TextWriter writer)
        {
            ValidationUtility.EnableDynamicValidation(context);
            context.Request.ValidateInput();
            HttpContextBase httpContextBase = new HttpContextWrapper(context);
            WebPageRenderingBase startPage = StartPage.GetStartPage(
                page, "_PageStart", WebPageHttpHandler.GetRegisteredExtensions());
            page.Context = httpContextBase;
            page.ExecutePageHierarchy(
                new WebPageContext(httpContextBase, page, null), writer, startPage);
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

        private void RenderTarget(IHttpEntity response, RazorViewBase target)
        {
            Encoding targetEncoding = Encoding.UTF8;
            response.ContentType.CharSet = targetEncoding.HeaderName;
            TextWriter writer = null;
            bool isNewWriter = false;
            try
            {
                var responseWithOwnTextWriter = response as ISupportsTextWriter;
                if (responseWithOwnTextWriter != null)
                {
                    writer = responseWithOwnTextWriter.TextWriter;
                }
                else
                {
                    writer = new DeterministicStreamWriter(response.Stream, targetEncoding, StreamActionOnDispose.None);
                    isNewWriter = true;
                }

                this.ProcessRequestInternal(HttpContext.Current, target, writer);
            }
            finally
            {
                var disposable = target as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }

                if (isNewWriter)
                {
                    writer.Dispose();
                }
            }
        }
    }
}