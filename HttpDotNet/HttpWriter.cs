using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HttpDotNet
{
    public class HttpWriter
    {
        public HttpConnectionStream Connection {get; protected set; }
        public Stream RawStream {get; protected set; }
        protected TextWriter Writer;

        public HttpWriter(Stream rawStream)
        {
            Connection = rawStream as HttpConnectionStream;
            RawStream = rawStream;
            Writer = new StreamWriter(rawStream, Encoding.ASCII)
            {
                NewLine = "\n"
            };
        }

        public void WriteMessage(HttpMessage message)
        {
            WriteGreeting(message);
            WriteHeaders(message);
            Writer.WriteLine("");
            Writer.Flush();
            message.BodyStream?.CopyTo(RawStream);
            RawStream.Flush();
        }

        protected void WriteGreeting(HttpMessage message)
        {
            if(message is HttpResponse response)
            {
                Writer.WriteLine($"HTTP/1.1 {response.StatusCode}");
            }
            else if(message is HttpRequest request)
            {
                Writer.WriteLine($"{request.Method} {request.Query} HTTP/1.1");
            }
        }

        protected void WriteHeaders(HttpMessage message)
        {
            foreach(var header in message.Headers)
            {
                Writer.WriteLine($"{header.Key}: {header.Value}");
            }
        }
    }
}