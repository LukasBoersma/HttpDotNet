using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace HttpDotNet.Tests
{
    [TestFixture]
    public class TestTransferEncoding
    {
        
        [Test]
        public void TestTransferEncodings_Identity()
        {
            var fullResponseBytes = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\n\r\nHello World");
            var message = Helper.ParseMessage(fullResponseBytes);
            var response = message as HttpResponse;
            Assert.NotNull(response);
            Assert.NotNull(response.BodyStream);
            Assert.AreEqual("Hello World", Encoding.ASCII.GetString(response.ReadBodyToEnd()));
        }
        
        [Test]
        public void TestTransferEncodings_Chunked_Empty()
        {
            var fullResponseBytes = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nTransfer-Encoding: chunked\r\n\r\n0\r\n\r\n");
            var message = Helper.ParseMessage(fullResponseBytes);
            var response = message as HttpResponse;
            Assert.NotNull(response);
            Assert.NotNull(response.BodyStream);
            Assert.AreEqual(response.ReadBodyToEnd().Length, 0);
        }
        
        [Test]
        public void TestTransferEncodings_Chunked_SingleByte()
        {
            var fullResponseBytes = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nTransfer-Encoding: chunked\r\n\r\n1\r\nx\r\n0\r\n\r\n");
            var message = Helper.ParseMessage(fullResponseBytes);
            var response = message as HttpResponse;
            Assert.NotNull(response);
            Assert.NotNull(response.BodyStream);
            Assert.IsInstanceOf<HttpContentStreamIdentity>(response.BodyStream);
            Assert.IsInstanceOf<HttpTransferStreamChunked>((response.BodyStream as HttpContentStreamIdentity).RawStream);
            var body = response.ReadBodyToEnd();
            Assert.AreEqual(1, body.Length);
            Assert.AreEqual("x", Encoding.ASCII.GetString(body));
        }
    }
}