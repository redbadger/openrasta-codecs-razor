namespace OpenRasta.Codecs.Razor
{
    public abstract class RazorViewBase<T> : RazorViewBase
    {
        private T resource;

        public T Resource
        {
            get
            {
                return this.resource;
            }
        }

        public override sealed void SetResource(object resource)
        {
            this.resource = (T)resource;
        }
    }
}