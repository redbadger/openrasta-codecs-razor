namespace OpenRasta.Codecs.Razor.ConsoleTestApp
{
    using System;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var h = new HttpListenerHostWithConfiguration { Configuration = new Configuration() };
            h.Initialize(new[] { "http://+:9222/" }, "/", null);
            h.StartListening();
            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }
    }
}