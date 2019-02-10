using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HttpDotNet
{
    
    public enum HttpProtocol
    {
        Http,
        Https,
        Auto

    }
    
    public class HttpConnectionStream: Stream
    {

        public Stream RawStream { get; set; }
        public Socket NetworkSocket { get; set; }
        public HttpConnectionStream(Stream rawStream, Socket networkSocket = null)
        {
            RawStream = rawStream;
            NetworkSocket = networkSocket;
        }

        public event EventHandler<HttpMessage> MessageParsed;

        public void ReadAllMessagesAsync()
        {
            Task.Run((Action)ReadAllMessages);
        }

        protected void ReadAllMessages()
        {
            if(!CanRead)
            {
                return;
            }

            Task<HttpMessage> messageTask = null;

            //todo: dispose parser
            try
            {
                var parser = new HttpParser(this);
                messageTask = parser.ParseMessageAsync();
                messageTask.Wait();
            }
            catch(System.IO.IOException e) { Debug.WriteLine(e); }
            catch(System.AggregateException e) { Debug.WriteLine(e); }
            
            if(messageTask?.IsCompletedSuccessfully ?? false)
            {
                MessageParsed?.Invoke(this, messageTask.Result);
                ReadAllMessagesAsync();
            }
        }

        public HttpMessage ReadMessage()
        {
            return HttpMessage.ReadMessage(this);
        }

        public void WriteMessage(HttpMessage message)
        {
            message.WriteMessageToStream(this);
        }

        public bool KeepAlive { get; set; } = false;

        public override bool CanRead => RawStream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => RawStream.CanWrite;

        public override long Length => RawStream.Length;

        public override long Position { get => RawStream.Position; set => RawStream.Position = value; }

        public override void Flush()
        {
            RawStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) => RawStream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => RawStream.Seek(offset, origin);

        public override void SetLength(long value) => RawStream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => RawStream.Write(buffer, offset, count);

        public static (int, HttpProtocol) ResolveAutoProtocolAndPort(int port, HttpProtocol protocol)
        {
            if(port < 0)
            {
                if(protocol == HttpProtocol.Https)
                {
                    port = 443;
                }
                else
                {
                    port = 80;
                }
            }

            if(protocol == HttpProtocol.Auto)
            {
                if(port == 443)
                {
                    protocol = HttpProtocol.Https;
                }
                else
                {
                    protocol = HttpProtocol.Http;
                }
            }

            return (port, protocol);
        }

        public static HttpConnectionStream ConnectToServer(string hostName, int port = -1, HttpProtocol protocol = HttpProtocol.Auto)
        {
            (port, protocol) = ResolveAutoProtocolAndPort(port, protocol);
            
            if(protocol == HttpProtocol.Https)
            {
                var tcpConnection = new TcpClient(hostName, port);
                var secureStream = new System.Net.Security.SslStream(tcpConnection.GetStream());
                secureStream.AuthenticateAsClient(hostName);
                return new HttpConnectionStream(secureStream, tcpConnection.Client);
            }
            else if(protocol == HttpProtocol.Http)
            {
                var tcpClient = new TcpClient(hostName, port);
                return new HttpConnectionStream(tcpClient.GetStream(), tcpClient.Client);
            }
            else
            {
                throw new NotSupportedException($"Unknown protocol '{protocol}' specified");
            }
        }
    }
}