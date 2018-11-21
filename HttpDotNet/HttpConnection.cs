using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace HttpDotNet
{
    public class HttpConnection: NetworkStream
    {
        public HttpConnection(Socket socket): base(socket)
        {
            Writer = new HttpWriter(this);
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
            var parser = new HttpParser(this);
            var resultTask = parser.ParseMessageAsync();
            resultTask.Wait();
            return resultTask.Result;
        }

        public void WriteMessage(HttpMessage message)
        {
            Writer.WriteMessage(message);
        }

        public HttpWriter Writer { get; protected set; }

        public Socket NetworkSocket => Socket;
        public bool Connected => NetworkSocket.Connected;

        public static HttpConnection ConnectToServer(string hostName, int port = 80)
        {
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(hostName, port);
            return new HttpConnection(socket);
        }
    }
}