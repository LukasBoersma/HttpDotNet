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
        public long? ContentLength { get; protected set; }

        public HttpContentStream(Stream rawStream, long? contentLength)
        {
            RawStream = rawStream;
            ContentLength = contentLength;
        }
        
        public static HttpContentStream Create(Stream rawStream, HttpTransferEncoding encoding, long? contentLength)
        {
            switch(encoding)
            {
                case HttpTransferEncoding.Identity:
                    return new HttpContentStreamIdentity(rawStream, contentLength);
                case HttpTransferEncoding.Chunked:
                    return new HttpContentStreamChunked(rawStream, contentLength);
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
    
    public class HttpContentStreamChunked: HttpContentStream
    {
        public HttpContentStreamChunked(Stream rawStream, long? contentLength): base(rawStream, contentLength)
        {
            ContentLength = contentLength;
        }
        
        long CurrentChunkLength = 0;
        long CurrentChunkRemaining = 0;
        
        public override int Read(byte[] buffer, int offset, int length)
        {
            int totalRead = 0;
            
            while(totalRead < length)
            {
                if(CurrentChunkRemaining == 0)
                {
                    if(CurrentChunkLength > 0)
                    {
                        // Read the empty line that follows the previous chunk
                        HttpLineReader.ReadLine(RawStream);
                    }
                    
                    NextChunk();
                    if(CurrentChunkLength == 0)
                    {
                        break;
                    }
                }
                
                int maxLength = (int)Math.Min(length - totalRead, CurrentChunkRemaining);
                int actualLength = RawStream.Read(buffer, offset, maxLength);
                totalRead += actualLength;
                offset += actualLength;
                
                CurrentChunkRemaining -= actualLength;
                
                if(actualLength == 0)
                {
                    break;
                }
            }
            
            return totalRead;
        }
        
        private void NextChunk()
        {
            var chunkLengthHex = HttpLineReader.ReadLine(RawStream);
            if(String.IsNullOrEmpty(chunkLengthHex))
            {
                CurrentChunkLength = 0;
                CurrentChunkRemaining = 0;
            }
            else
            {
                CurrentChunkLength = Convert.ToInt64(chunkLengthHex, 16);
                CurrentChunkRemaining = CurrentChunkLength;
            }
        }
    }
}