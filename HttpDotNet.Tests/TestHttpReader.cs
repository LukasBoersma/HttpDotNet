using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Nito.AsyncEx;

namespace HttpDotNet.Tests
{
    [TestFixture]
    public class TestHttpReader
    {
        static HttpMessage ParseMessage(byte[] rawData)
        {
            var fakeHttpStream = new MemoryStream(rawData);
            fakeHttpStream.Seek(0, SeekOrigin.Begin);
            var parser = new HttpParser(fakeHttpStream);
            return AsyncContext.Run(() => parser.ParseMessageAsync());
        }
        
        [Test]
        public void TestHttpReader_200Response()
        {
            var bodyString = "Hello World! What a nice day it is today.";
            var expectedBodyBytes = Encoding.ASCII.GetBytes(bodyString);
            var fullResponseBytes = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\n\r\n" + bodyString);
            var message = ParseMessage(fullResponseBytes);
            var response = message as HttpResponse;
            Assert.NotNull(response);
            Assert.NotNull(response.BodyStream);
            Assert.AreEqual(response.StatusCode, "200 OK");
            Assert.AreEqual(response.ReadBodyToEnd(), expectedBodyBytes);
        }
        
        [Test]
        public void TestHttpReader_BrokenResponse()
        {
            var brokenResponseBytes = Encoding.ASCII.GetBytes("HTTP/1(/SHD(I)H");
            Assert.Throws<InvalidDataException>(() => {
                ParseMessage(brokenResponseBytes);
            });
        }
    }
}