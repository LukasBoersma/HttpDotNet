using System;
using System.Net;
using System.Net.Sockets;

namespace HttpDotNet.Tests
{
    public class FakeHttpConnection: HttpConnection
    {
        public FakeHttpConnection(Socket socket): base(socket)
        {
        }
        
        public static FakeHttpConnection Create()
        {
            var fakeSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            fakeSocket.Connect(new IPEndPoint(IPAddress.IPv6Loopback, 16888));
            return new FakeHttpConnection(fakeSocket);
        }
    }
}