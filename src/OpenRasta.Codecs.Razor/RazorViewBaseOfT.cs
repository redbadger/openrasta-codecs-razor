namespace OpenRasta.Codecs.Razor
{
    public abstract class RazorViewBase<T> : RazorViewBase
    {
        public T Resource { get; private set; }

        public override sealed void SetResource(object resource)
        {
            this.Resource = (T)resource;
        }
    }
}