using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HttpDotNet
{
    
    public class HttpMessage
    {
        public HttpConnectionStream Connection {get; set;}
        public Stream BodyStream {get; set;}
        public Dictionary<string, string> Headers {get; set;} = new Dictionary<string, string>();

        public void SetBody(byte[] data)
        {
            BodyStream = new MemoryStream(data);
        }

        public void SetBody(string data, Encoding encoding = null)
        {
            if(encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            SetBody(encoding.GetBytes(data));
        }

        public void SetBody(Stream stream)
        {
            BodyStream = stream;
        }
        
        public byte[] ReadBodyToEnd()
        {
            return BodyStream.ReadAllBytes();
        }

        public string this[string headerName]
        {
            get
            {
                if(TryGetHeader(headerName.ToLower(), out string header))
                {
                    return header;
                }
                else
                {
                    return null;
                }
            }
            set => Headers[headerName.ToLower()] = value;
        }
        
        public bool TryGetHeader(string headerName, out string headerValue)
        {
            headerName = headerName.ToLower();
            if(Headers.ContainsKey(headerName))
            {
                headerValue = Headers[headerName];
                return true;
            }
            else
            {
                headerValue = null;
                return false;
            }
        }

        public static HttpMessage ReadMessage(Stream stream)
        {
            var parser = new HttpParser(stream);
            var resultTask = parser.ParseMessageAsync();
            return resultTask.Result;
        }

        public void WriteMessageToStream(Stream stream)
        {
            var writer = new HttpWriter(stream);
            writer.WriteMessage(this);
        }
    }

    public class HttpResponse: HttpMessage
    {
        public string StatusCode;
    }
    
    public class HttpRequest: HttpMessage
    {
        public static bool SetDefaultHeaderForAcceptEncoding { get; set; } = true;
        public static bool SetDefaultHeaderForKeepAlive { get; set; } = true;

        public HttpRequest()
        {
            if(SetDefaultHeaderForAcceptEncoding)
            {
                Headers["accept-encoding"] = "gzip,identity";
                Headers["te"] = "chunked,gzip,identity";
            }
            if(SetDefaultHeaderForKeepAlive)
            {
                Headers["connection"] = "keep-alive";
            }
        }

        public string Method;
        public string Query;
    }
}