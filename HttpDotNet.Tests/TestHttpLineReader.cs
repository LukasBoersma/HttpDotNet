using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Nito.AsyncEx;

namespace HttpDotNet.Tests
{
    [TestFixture]
    public class TestHttpLineReader
    {
        static string[] ReadAllLines(string asciiInput)
        {
            var inputBytes = Encoding.ASCII.GetBytes(asciiInput);
            using (var memStream = new MemoryStream(inputBytes))
            {
                memStream.Seek(0, SeekOrigin.Begin);
                var lines = new List<string>();
                string line = null;
                while((line = HttpLineReader.ReadLine(memStream)) != null)
                {
                    lines.Add(line);
                }
                return lines.ToArray();
            }
        }
        
        [Test]
        public void TestHttpLineReader_EmptyLines()
        {
            {
                var lines = ReadAllLines("\r\n");
                Assert.AreEqual(1, lines.Length);
                Assert.AreEqual(0, lines[0].Length);
            }
            
            {
                var lines = ReadAllLines("\r\n\r\n");
                Assert.AreEqual(2, lines.Length);
                Assert.AreEqual(0, lines[0].Length);
                Assert.AreEqual(0, lines[1].Length);
            }
            
            {
                var lines = ReadAllLines("");
                Assert.AreEqual(0, lines.Length);
            }
        }
        
        [Test]
        public void TestHttpLineReader_NormalLines()
        {
            {
                var lines = ReadAllLines("Hello\r\nWorld\r\n!");
                Assert.AreEqual(3, lines.Length);
                Assert.AreEqual("Hello", lines[0]);
                Assert.AreEqual("World", lines[1]);
                Assert.AreEqual("!", lines[2]);
            }
        }
        
        [Test]
        public void TestHttpLineReader_ChunkedEncoding()
        {
            {
                var lines = ReadAllLines("HTTP/1.1 200 OK\r\nTransfer-Encoding: chunked\r\n0\r\n\r\n");
                Assert.AreEqual(4, lines.Length);
                Assert.AreEqual("0", lines[2]);
                Assert.AreEqual("", lines[3]);
            }
        }
        
        
    }
}