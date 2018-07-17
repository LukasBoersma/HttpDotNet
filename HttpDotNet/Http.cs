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
        public HttpConnection Connection {get; set;}
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

        public string this[string headerName]
        {
            get => Headers[headerName];
            set => Headers[headerName] = value;
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
        public TextReader Reader { get; protected set; }
        public HttpConnection Connection { get; protected set; }
        public HttpParser(HttpConnection connection)
        {
            Connection = connection;
            Reader = new StreamReader(connection);
        }

        public static async Task<HttpMessage> ParseMessage(HttpConnection connection)
        {
            var parser = new HttpParser(connection);
            return await parser.ParseMessageAsync();
        }

        public async Task<HttpMessage> ParseMessageAsync()
        {
            var message = await ReadGreeting();
            if(message == null)
                throw new InvalidDataException("Reading HTTP greeting failed");

            message.Headers = await ReadAllHeaders();
            return message;
        }

        protected async Task<HttpMessage> ReadGreeting()
        {
            var greeting = await Reader.ReadLineAsync();
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
                    BodyStream = Connection,
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
                    BodyStream = Connection,
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
            while((line = await Reader.ReadLineAsync()) != null)
            {
                Match headerMatch;

                if(String.IsNullOrEmpty(line))
                {
                    break;
                }
                else if((headerMatch = HeaderPattern.Match(line)).Success)
                {
                    headers[headerMatch.Groups["Name"].Value] = headerMatch.Groups["Value"].Value;
                }
            }

            return headers;
        }
    }

    public class HttpWriter
    {
        public HttpConnection Connection {get; protected set; }
        protected TextWriter Writer;

        public HttpWriter(HttpConnection connection)
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