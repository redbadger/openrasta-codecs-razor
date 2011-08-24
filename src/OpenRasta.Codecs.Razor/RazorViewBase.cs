namespace OpenRasta.Codecs.Razor
{
    using System.Collections.Generic;
    using System.Web.WebPages;

    using OpenRasta.DI;

    public abstract class RazorViewBase : WebPageBase
    {
        public IList<Error> Errors { get; set; }

        public IDependencyResolver Resolver
        {
            get
            {
                return DependencyManager.GetService<IDependencyResolver>();
            }
        }

        public abstract void SetResource(object resource);
    }
}