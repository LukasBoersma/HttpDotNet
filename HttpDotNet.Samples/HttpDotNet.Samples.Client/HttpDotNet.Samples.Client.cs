using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace HttpDotNet.Samples.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // HttpClient example
            Console.WriteLine("Request to http://jigsaw.w3.org/HTTP/connection.html gave this response:");
            var result = HttpClient.GetString(new Uri("http://jigsaw.w3.org/HTTP/connection.html"));
            Console.WriteLine(result);
        }
    }
}
