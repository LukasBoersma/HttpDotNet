using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace HttpDotNet
{
    public class HttpClient: IDisposable
    {
        private HttpRawConnectionStream Connection;
        public HttpClient(string hostName, int port = 80)
        {
            Connection = HttpRawConnectionStream.ConnectToServer(hostName, port);
        }

        public HttpResponse SendRequest(HttpRequest request)
        {
            Connection.WriteMessage(request);
            return Connection.ReadMessage() as HttpResponse;
        }

        public static HttpResponse SendRequest(string hostName, int port, HttpRequest request)
        {
            var client = new HttpClient(hostName, port);
            return client.SendRequest(request);
        }
        #region  Convenience methods

        public static HttpResponse GetResponse(Uri uri)
        {
            int port = 80;
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

            return SendRequest(uri.Host, port, request);
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