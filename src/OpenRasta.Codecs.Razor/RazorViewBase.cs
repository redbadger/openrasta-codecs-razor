namespace OpenRasta.Codecs.Razor
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Web;
    using System.Web.WebPages;

    public abstract class RazorViewBase
    {
        public IList<Error> Errors { get; set; }

        public TextWriter Output { get; set; }

        /// <summary>
        ///     This method is called by generated code and needs to stay in sync with the parser
        /// </summary>
        public static void WriteLiteralTo(TextWriter writer, object content)
        {
            writer.Write(content);
        }

        /// <summary>
        ///     This method is called by generated code and needs to stay in sync with the parser
        /// </summary>
        public static void WriteTo(TextWriter writer, HelperResult content)
        {
            if (content != null)
            {
                content.WriteTo(writer);
            }
        }

        /// <summary>
        ///     This method is called by generated code and needs to stay in sync with the parser
        /// </summary>
        public static void WriteTo(TextWriter writer, object content)
        {
            writer.Write(HttpUtility.HtmlEncode(content));
        }

        /// <summary>
        ///     Overridden in generated view class.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract void Execute();

        public abstract void SetResource(object resource);

        public void Write(HelperResult result)
        {
            WriteTo(this.Output, result);
        }

        public void Write(object value)
        {
            WriteTo(this.Output, value);
        }

        public void WriteLiteral(object value)
        {
            this.Output.Write(value);
        }
    }
}