using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace HttpDotNet.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            //Use the InvariantCulture locale, making sure this code produces the same results on every machine.
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            // HttpClient example
            Console.WriteLine("Request to http://jigsaw.w3.org/HTTP/ChunkedScript gave this response:");
            var result = HttpClient.GetString(new Uri("http://jigsaw.w3.org/HTTP/ChunkedScript"));
            Console.WriteLine(result);

            // HttpListener example
            Console.WriteLine("Starting Server...");
            var server = new HttpListener();

            // Register the RequestParsed event so we can react to incoming requests
            server.RequestParsed += (sender, request) =>
            {
                var remoteAddress = request.Connection.NetworkSocket.RemoteEndPoint;
                // Generate the response body
                var responseText = $"<h1>Hello, {remoteAddress}!</h1><p>You requested '{request.Query}', and this is your response.</p>\n";
                var responseBytes = Encoding.UTF8.GetBytes(responseText);

                // Create and configure a response object
                var response = new HttpResponse();
                response.StatusCode = HttpStatusCodes.OK;
                response["Content-Type"] = "text/html; charset=utf-8";
                response["Content-Length"] = responseBytes.Length.ToString();
                response.BodyStream = new MemoryStream(responseBytes);

                // Send the response to the client
                request.Connection.WriteMessage(response);
                request.Connection.Close();
            };

            // Listen on localhost (loopback). This way, only local connections will be accepted.
            // Change to IPAddress.Any to allow connections from any source.
            var listeningEndpoint = new IPEndPoint(address: IPAddress.Loopback, port: 8888);
            server.StartListening(listeningEndpoint);

            // Start running the server. This method never returns!
            // Use RunInBackground if you want to continue doing work in this thread.
            server.RunBlocking();
        }
    }
}
