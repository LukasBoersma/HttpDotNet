using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace HttpDotNet
{
    public class HttpClient: IDisposable
    {
        private HttpConnectionStream Connection;
        private Stream RawStream;

        public HttpClient(string hostName, int port = -1, HttpProtocol protocol = HttpProtocol.Auto)
        {
            Connection = HttpConnectionStream.ConnectToServer(hostName, port, protocol);
            RawStream = Connection;
        }

        public HttpClient(Stream customStream)
        {
            // If the given stream is a HttpConnectionStream set it as the connection, otherwise leave connection null;
            Connection = customStream as HttpConnectionStream;
            RawStream = customStream;
        }

        public HttpResponse SendRequest(HttpRequest request)
        {
            if(Connection != null)
            {
                Connection.WriteMessage(request);
                return Connection.ReadMessage() as HttpResponse;
            }
            else
            {
                var writer = new HttpWriter(RawStream);
                writer.WriteMessage(request);
                var parser = new HttpParser(RawStream);
                return parser.ParseMessageAsync().Result as HttpResponse;
            }
        }

        public static HttpResponse SendRequest(HttpRequest request, string hostName, int port = -1, HttpProtocol protocol = HttpProtocol.Auto)
        {
            var client = new HttpClient(hostName, port, protocol);
            return client.SendRequest(request);
        }

        #region  Convenience methods

        public static HttpResponse GetResponse(string uri) => GetResponse(new Uri(uri))
        public static HttpResponse GetResponse(Uri uri)
        {
            int port = 80;
            var protocol = HttpProtocol.Http;
            
            if(uri.Scheme.ToLower() == "https")
            {
                protocol = HttpProtocol.Https;
                port = 443;
            }

            if(uri.Port != -1)
            {
                port = uri.Port;
            }

            var query = uri.PathAndQuery;
            if(String.IsNullOrEmpty(query))
            {
                query = "/";
            }

            var request = new HttpRequest
            {
                Method = "GET",
                Query = query
            };

            request["Host"] = uri.Host;

            return SendRequest(request, uri.Host, port, protocol);
        }

        private static bool GetContentLength(HttpResponse response, out int contentLength)
        {
            contentLength = int.MaxValue;
            if(response.Headers.ContainsKey("Content-Length"))
            {
                if(int.TryParse(response["Content-Length"], out contentLength))
                {
                   return true;
                }
            }
            return false;
        }

        public static byte[] GetBytes(string uri) => GetBytes(new Uri(uri));

        public static byte[] GetBytes(Uri uri)
        {
            HttpResponse response = GetResponse(uri);
            
            var reader = new BinaryReader(response.BodyStream);
            byte[] data;
            int contentLength;
            if(GetContentLength(response, out contentLength))
            {
                data = reader.ReadBytes(contentLength);
            }
            else
            {
                data = reader.ReadAllBytes();
            }
            
            reader.Dispose();
            response.BodyStream.Dispose();

            return data;
        }

        private static Regex CharsetRegex = new Regex(@"^.+; ?charset=(<charsetName>.+)$");

        public static string GetString(string uri, Encoding encoding = null) => GetString(new Uri(uri), encoding);
        public static string GetString(Uri uri, Encoding encoding = null)
        {
            HttpResponse response = GetResponse(uri);

            if(encoding == null)
            {
                encoding = Encoding.UTF8;
                Match charsetMatch = null;
                if(response.Headers.ContainsKey("Content-Type") && (charsetMatch = CharsetRegex.Match(response["Content-Type"])).Success)
                {
                    var charsetName = charsetMatch.Groups["charsetName"].Value;
                    encoding = Encoding.GetEncoding(charsetName);
                }
            }

            var reader = new BinaryReader(response.BodyStream);
            
            byte[] data;
            int contentLength;
            if(GetContentLength(response, out contentLength))
            {
                data = reader.ReadBytes(contentLength);
            }
            else
            {
                data = reader.ReadAllBytes();
            }

            var stringData = encoding.GetString(data);

            reader.Dispose();
            response.BodyStream.Dispose();

            return stringData;
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Connection.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}