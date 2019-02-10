using System;
using System.IO;
using System.IO.Compression;

namespace HttpDotNet
{
    public enum HttpContentEncoding
    {
        Identity,
        Gzip,
        Unsupported
    }
    
    public abstract class HttpContentStream: Stream
    {
        public static HttpContentEncoding EncodingFromHeaderValue(string encodingValue)
        {
            switch(encodingValue)
            {
                case null:
                    return HttpContentEncoding.Identity;
                case "":
                    return HttpContentEncoding.Identity;
                case "identity":
                    return HttpContentEncoding.Identity;
                case "gzip":
                    return HttpContentEncoding.Gzip;
                default:
                    return HttpContentEncoding.Unsupported;
            }
        }

        public Stream RawStream { get; protected set; }
        public long? ContentLength { get; protected set; }

        public HttpContentStream(Stream rawStream, long? contentLength)
        {
            RawStream = rawStream;
            ContentLength = contentLength;
        }
        
        public static HttpContentStream Create(Stream rawStream, HttpContentEncoding encoding, long? contentLength)
        {
            switch(encoding)
            {
                case HttpContentEncoding.Identity:
                    return new HttpContentStreamIdentity(rawStream, contentLength);
                case HttpContentEncoding.Gzip:
                    return new HttpContentStreamGzip(rawStream, contentLength);
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

        public virtual bool HasLength => ContentLength != null;
        public override long Length => ContentLength ?? throw new NotSupportedException("This HttpContentStream has unknown length. Check HasLength before reading the Length property.");

        public override bool CanRead => RawStream.CanRead;
        public override bool CanWrite => false;
        public override bool CanSeek => false;
        public override bool CanTimeout => false;
        public override long Position 
        {
            get => RawStream.Position;
            set => RawStream.Position = value;
        }
    }

    public class HttpContentStreamIdentity: HttpContentStream
    {
        public HttpContentStreamIdentity(Stream rawStream, long? contentLength): base(rawStream, contentLength)
        {
        }

        public override int Read(byte[] buffer, int offset, int length)
        {
            // Todo: count already read bytes from previous Read calls and use it to compute remaining expected length
            int maxLength = (int)Math.Min(length, ContentLength ?? length);
            int actualLength = RawStream.Read(buffer, offset, maxLength);
            return actualLength;
        }
    }

    public class HttpContentStreamGzip: HttpContentStream
    {
        GZipStream GZip;
        public HttpContentStreamGzip(Stream rawStream, long? contentLength): base(rawStream, contentLength)
        {
            GZip = new GZipStream(rawStream, CompressionMode.Decompress);
        }

        public override bool CanRead => RawStream.CanRead;

        public override long Position 
        {
            get => GZip.Position;
            set => throw new NotSupportedException("Cannot seek in gzipped streams.");
        }

        public override int Read(byte[] buffer, int offset, int length)
        {
            if(!GZip.CanRead)
                return 0;

            // Todo: count already read bytes from previous Read calls and use it to compute remaining expected length
            int maxLength = (int)Math.Min(length, ContentLength ?? length);
            int actualLength = GZip.Read(buffer, offset, maxLength);

            if(actualLength == 0)
            {
                Close();
            }

            return actualLength;
        }
    }
}