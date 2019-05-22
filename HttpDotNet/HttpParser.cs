using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HttpDotNet
{
    public class HttpParser
    {
        public HttpConnectionStream Connection { get; protected set; }
        public Stream RawStream { get; protected set; }
        public bool ThrowOnBadData { get; set; } = true;
        
        public HttpParser(Stream rawStream)
        {
            RawStream = rawStream;
            Connection = rawStream as HttpConnectionStream;
        }

        public static async Task<HttpMessage> ParseMessage(HttpConnectionStream connection)
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
            
            // Handle encodings
            message.TryGetHeader("transfer-encoding", out var transferEncodingString);
            var transferEncoding = HttpTransferStream.EncodingFromHeaderValue(transferEncodingString);
            var transferStream = HttpTransferStream.Create(RawStream, transferEncoding, contentLength);

            message.TryGetHeader("content-encoding", out var contentEncodingString);
            var contentEncoding = HttpContentStream.EncodingFromHeaderValue(contentEncodingString);
            message.BodyStream = HttpContentStream.Create(transferStream, contentEncoding, contentLength);

            // Pass Keep-Alive information to HttpRawConnectionStream
            message.TryGetHeader("connection", out var connectionHeaderString);
            if(Connection != null)
            {
                // If the header "Connection" contains "keep-alive" in its header list, the http connection is set to keep-alive
                // Whenever we encounter a message without keep-alive set, the keep-alive is disabled for the connection and it will
                // be closed after receiving the message.
                Connection.KeepAlive = KeepAliveHeaderPattern.IsMatch(connectionHeaderString.ToLower());
            }

            return message;
        }

        protected async Task<HttpMessage> ReadGreeting()
        {
            var greeting = await HttpLineReader.ReadLineAsync(RawStream);
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
        static readonly Regex KeepAliveHeaderPattern = new Regex(@"^([a-z,\-]+,)?keep-alive(,[a-z,\-]+)?$");

        protected async Task<Dictionary<string, string>> ReadAllHeaders()
        {
            var headers = new Dictionary<string, string>();
            string line;
            while((line = await HttpLineReader.ReadLineAsync(RawStream)) != null)
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
}