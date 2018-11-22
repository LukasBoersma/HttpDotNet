using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace HttpDotNet.Tests
{
    [TestFixture]
    public class TestHttpReader
    {
        
        [Test]
        public void TestHttpReader_200Response()
        {
            var bodyString = "Hello World! What a nice day it is today.";
            var expectedBodyBytes = Encoding.ASCII.GetBytes(bodyString);
            var fullResponseBytes = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\n\r\n" + bodyString);
            var message = Helper.ParseMessage(fullResponseBytes);
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
                Helper.ParseMessage(brokenResponseBytes);
            });
        }
    }
}