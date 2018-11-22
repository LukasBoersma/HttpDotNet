using System;
using NUnit.Framework;

namespace HttpDotNet.Tests
{
    [TestFixture]
    public class TestHttpStatusCodes
    {
        [Test]
        public void TestHttpStatusCodes_GetCodeNumber()
        {
            Assert.AreEqual(404, HttpDotNet.HttpStatusCodes.GetCodeNumber("404 Not Found"));
            Assert.AreEqual(404, HttpStatusCodes.GetCodeNumber("404 Something Else"));
            Assert.AreEqual(404, HttpStatusCodes.GetCodeNumber("404"));
        }
        
        [Test]
        public void TestHttpStatusCodes_GetCodeNumber_BrokenCodes()
        {
            Assert.AreEqual(-1, HttpStatusCodes.GetCodeNumber("40"));
            Assert.AreEqual(-1, HttpStatusCodes.GetCodeNumber("Not Found"));
            Assert.AreEqual(-1, HttpStatusCodes.GetCodeNumber("5000 Something"));
            Assert.AreEqual(-1, HttpStatusCodes.GetCodeNumber(""));
            Assert.AreEqual(-1, HttpStatusCodes.GetCodeNumber(null));
        }
    }
}