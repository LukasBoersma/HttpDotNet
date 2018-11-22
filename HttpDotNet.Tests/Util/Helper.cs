using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Nito.AsyncEx;

namespace HttpDotNet.Tests
{
    public static class Helper
    {        
        public static HttpMessage ParseMessage(byte[] rawData)
        {
            var fakeHttpStream = new MemoryStream(rawData);
            fakeHttpStream.Seek(0, SeekOrigin.Begin);
            var parser = new HttpParser(fakeHttpStream);
            return AsyncContext.Run(() => parser.ParseMessageAsync());
        }
    }
}