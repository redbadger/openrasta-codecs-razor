namespace OpenRasta.Codecs.Razor
{
    using System.Collections.Generic;
    using System.Web;
    using System.Web.WebPages;

    using OpenRasta.DI;
    using OpenRasta.Web.Markup;

    public abstract class RazorViewBase : WebPageBase
    {
        private XhtmlAnchor xhtmlAnchor;

        public IList<Error> Errors { get; set; }

        public IDependencyResolver Resolver
        {
            get
            {
                return DependencyManager.GetService<IDependencyResolver>();
            }
        }

        public IXhtmlAnchor Xhtml
        {
            get
            {
                return this.xhtmlAnchor ??
                       (this.xhtmlAnchor =
                        new XhtmlAnchor(this.Resolver, new XhtmlTextWriter(this.Output), () => HttpContext.Current.User));
            }
        }

        public abstract void SetResource(object resource);
    }
}