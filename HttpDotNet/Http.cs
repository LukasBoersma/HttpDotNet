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
        public HttpRawConnectionStream Connection {get; set;}
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
            get => Headers[headerName.ToLower()];
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
    }

    public class HttpResponse: HttpMessage
    {
        public string StatusCode;
    }
    
    public class HttpRequest: HttpMessage
    {
        public string Method;
        public string Query;
    }
    
    public class HttpParser
    {
        public HttpRawConnectionStream Connection { get; protected set; }
        public Stream RawStream { get; protected set; }
        public bool ThrowOnBadData { get; set; } = true;
        
        public HttpParser(Stream rawStream)
        {
            RawStream = rawStream;
            Connection = rawStream as HttpRawConnectionStream;
        }

        public static async Task<HttpMessage> ParseMessage(HttpRawConnectionStream connection)
        {
            var parser = new HttpParser(connection);
            return await parser.ParseMessageAsync();
        }

        public async Task<HttpMessage> ParseMessageAsync()
        {
            var message = await ReadGreeting();
            if(message == null)
            {
                if(ThrowOnBadData)
                {
                    throw new InvalidDataException("Reading HTTP greeting failed");
                }
                else
                {
                    return null;
                }
            }

            message.Headers = await ReadAllHeaders();
            
            message.TryGetHeader("content-length", out var contentLengthString);
            int? contentLength = null;
            if(contentLengthString != null && int.TryParse(contentLengthString, out int contentLengthValue))
            {
                contentLength = contentLengthValue;
            }
            
            message.TryGetHeader("transfer-encoding", out var transferEncodingString);
            
            var transferEncoding = HttpContentStream.EncodingFromHeaderValue(transferEncodingString);

            message.BodyStream = HttpContentStream.Create(RawStream, transferEncoding, contentLength);

            return message;
        }

        protected async Task<HttpMessage> ReadGreeting()
        {
            var greeting = HttpLineReader.ReadLine(RawStream);
            if(greeting == null)
                return null;

            Match greetingMatch;
            if((greetingMatch = RequestGreetingPattern.Match(greeting)).Success)
            {
                var method = greetingMatch.Groups["Method"].Value;
                var query = greetingMatch.Groups["Query"].Value;
                return new HttpRequest
                {
                    Connection = Connection,
                    Method = method,
                    Query = query,
                };
            }
            else if((greetingMatch = ResponseGreetingPattern.Match(greeting)).Success)
            {
                var statusCode = greetingMatch.Groups["StatusCode"].Value;
                return new HttpResponse
                {
                    Connection = Connection,
                    StatusCode = statusCode,
                };
            }
            else
            {
                throw new InvalidDataException("HTTP greeting not understood.");
            }
        }

        static readonly Regex HeaderPattern = new Regex(@"^(?<Name>[a-zA-Z0-9\-]+)[ \t]*:[ \t]*(?<Value>.*)$");
        static readonly Regex RequestGreetingPattern = new Regex(@"^(?<Method>[A-Z]+) (?<Query>[a-zA-Z0-9\-_./%+]*) HTTP/1\.(0|1)$");
        static readonly Regex ResponseGreetingPattern = new Regex(@"^HTTP/1\.(0|1) (?<StatusCode>[0-9]+ [A-Za-z\- ]+)$");

        protected async Task<Dictionary<string, string>> ReadAllHeaders()
        {
            var headers = new Dictionary<string, string>();
            string line;
            while((line = HttpLineReader.ReadLine(RawStream)) != null)
            {
                Match headerMatch;
                if(String.IsNullOrEmpty(line))
                {
                    break;
                }
                else if((headerMatch = HeaderPattern.Match(line)).Success)
                {
                    // header names are always converted to lower case
                    var headerName = headerMatch.Groups["Name"].Value.ToLower();
                    var headerValue = headerMatch.Groups["Value"].Value;
                    headers[headerName] = headerValue;
                }
            }

            return headers;
        }
    }

    public class HttpWriter
    {
        public HttpRawConnectionStream Connection {get; protected set; }
        protected TextWriter Writer;

        public HttpWriter(HttpRawConnectionStream connection)
        {
            Connection = connection;
            Writer = new StreamWriter(connection, Encoding.ASCII)
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
            message.BodyStream?.CopyTo(Connection);
            Connection.Flush();
        }

        protected void WriteGreeting(HttpMessage message)
        {
            if(message is HttpResponse response)
            {
                Writer.WriteLine($"HTTP/1.0 {response.StatusCode}");
            }
            else if(message is HttpRequest request)
            {
                Writer.WriteLine($"{request.Method} {request.Query} HTTP/1.0");
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