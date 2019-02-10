using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace HttpDotNet.Samples.FileDownload
{
    class Program
    {
        static void Main(string[] args)
        {
            var response = HttpClient.GetResponse("https://lukas-boersma.com/en/");
            using(var fileStream = File.OpenWrite("downloaded.html"))
            {
                response.BodyStream.CopyTo(fileStream);
            }
        }
    }
}
