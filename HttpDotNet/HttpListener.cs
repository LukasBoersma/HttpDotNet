using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace HttpDotNet
{
    public class HttpListener
    {
        private Socket ListeningSocket;
        private Task<Socket> AcceptTask;
        private List<HttpStream> OpenConnections = new List<HttpStream>();

        public event EventHandler<HttpMessage> MessageParsed;
        public event EventHandler<HttpRequest> RequestParsed;
        public event EventHandler<HttpResponse> ResponseParsed;

        protected void OnMessageParsed(object sender, HttpMessage message)
        {
            MessageParsed?.Invoke(sender, message);
            if(message is HttpRequest request)
            {
                RequestParsed?.Invoke(sender, request);
            }
            else if(message is HttpResponse response)
            {
                ResponseParsed?.Invoke(sender, response);
            }
        }

        public void StartListening(IPEndPoint endPoint)
        {
            if(ListeningSocket != null)
            {
                throw new InvalidOperationException("HttpListener is already listening. Can't start listening again before stopping.");
            }

            ListeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ListeningSocket.Bind(endPoint);
            //Todo: Set timeouts
            ListeningSocket.Listen(1);
        }

        public void Process()
        {
            if(ListeningSocket == null)
            {
                return;
            }

            // New connection established successfully? Add it to OpenConnections
            if(AcceptTask != null && AcceptTask.IsCompleted)
            {
                if(AcceptTask.IsCompletedSuccessfully)
                {
                    var newHttpConnection = new HttpStream(AcceptTask.Result);
                    newHttpConnection.MessageParsed += OnMessageParsed;
                    newHttpConnection.ReadAllMessagesAsync();
                    OpenConnections.Add(newHttpConnection);
                }

                AcceptTask.Dispose();
                AcceptTask = null;
            }
            if(AcceptTask == null)
            {
                AcceptTask = ListeningSocket.AcceptAsync();
            }

            var currentConnections = OpenConnections.ToArray();
            foreach(var connection in currentConnections)
            {
                if(!connection.Connected)
                    OpenConnections.Remove(connection);
            }
        }

        public void RunBlocking()
        {
            while(true)
            {
                Process();
                Thread.Sleep(0);
            }
        }

        public Thread RunInBackground()
        {
            var serverThread = new Thread(RunBlocking);
            serverThread.Start();
            return serverThread;
        }
    }
}