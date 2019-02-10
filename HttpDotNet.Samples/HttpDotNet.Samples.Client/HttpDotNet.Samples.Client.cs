using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HttpDotNet.Samples.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = new Uri("https://lukas-boersma.com/en/");
            var response = HttpClient.GetString(uri);
            Console.WriteLine($"Request to {uri} returned the following response:"); 
            Console.WriteLine(response);
        }
    }
}
