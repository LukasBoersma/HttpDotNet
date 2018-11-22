using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;

namespace HttpDotNet
{
    public enum HttpTransferEncoding
    {
        Identity,
        Chunked,
        Gzip
    }

    public abstract class HttpContentStream: Stream
    {
        public static HttpTransferEncoding EncodingFromHeaderValue(string encodingValue)
        {
            switch(encodingValue)
            {
                case null:
                    return HttpTransferEncoding.Identity;
                case "":
                    return HttpTransferEncoding.Identity;
                case "identity":
                    return HttpTransferEncoding.Identity;
                case "chunked":
                    return HttpTransferEncoding.Chunked;
                case "gzip":
                    return HttpTransferEncoding.Gzip;
                default:
                    throw new InvalidDataException("Transfer Encoding not understood or not supported.");
            }
        }
        
        public Stream RawStream { get; protected set; }
        
        public HttpContentStream(Stream rawStream)
        {
            RawStream = rawStream;
        }
        
        public static HttpContentStream Create(Stream rawStream, HttpTransferEncoding encoding, int? contentLength)
        {
            switch(encoding)
            {
                case HttpTransferEncoding.Identity:
                    return new HttpContentStreamIdentity(rawStream, contentLength);
                //case HttpTransferEncoding.Chunked:
                //    return new HttpContentStreamChunked(cawStream);
                default:
                    throw new NotImplementedException($"Transfer encoding {encoding} is not implemented.");
            }
        }
        
        // Stream Implementation: Reading only, no seeking
        
        public override void Flush () {}
        public override long Seek (long offset, System.IO.SeekOrigin origin)
            => throw new NotSupportedException("HttpContentStream does not support seeking");
        public override void SetLength (long value)
            => throw new NotSupportedException("HttpContentStream is read-only.");
        public override void Write (byte[] buffer, int offset, int count)
            => throw new NotSupportedException("HttpContentStream is read-only.");

        public override bool CanRead => RawStream.CanRead;
        public override bool CanWrite => false;
        public override bool CanSeek => false;
        public override bool CanTimeout => false;
        public override long Length => throw new NotSupportedException("HttpContentStream does not support reading length");
        public override long Position 
        {
            get => RawStream.Position;
            set => RawStream.Position = value;
        }
    }
    
    public class HttpContentStreamIdentity: HttpContentStream
    {
        public HttpContentStreamIdentity(Stream rawStream, int? contentLength): base(rawStream)
        {
            ContentLength = contentLength;
        }
        
        public int? ContentLength { get; protected set; }
        
        public override int Read(byte[] buffer, int offset, int length)
        {
            var maxLength = Math.Min(length, ContentLength ?? length);
            int actualLength = RawStream.Read(buffer, offset, maxLength);
            return actualLength;
        }
    }
}