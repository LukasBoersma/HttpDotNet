using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace HttpDotNet
{
    public enum HttpTransferEncoding
    {
        Identity,
        Chunked,
        Gzip,
        Unsupported
    }

    public abstract class HttpTransferStream: Stream
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
                    return HttpTransferEncoding.Unsupported;
            }
        }
        
        public Stream RawStream { get; protected set; }
        public long? ContentLength { get; protected set; }

        public HttpTransferStream(Stream rawStream, long? contentLength)
        {
            RawStream = rawStream;
            ContentLength = contentLength;
        }
        
        public static HttpTransferStream Create(Stream rawStream, HttpTransferEncoding encoding, long? contentLength)
        {
            switch(encoding)
            {
                case HttpTransferEncoding.Identity:
                    return new HttpTransferStreamIdentity(rawStream, contentLength);
                case HttpTransferEncoding.Chunked:
                    return new HttpTransferStreamChunked(rawStream, contentLength);
                case HttpTransferEncoding.Gzip:
                    return new HttpTransferStreamGzip(rawStream, contentLength);
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

        public override void Close()
        {
            base.Close();
            RawStream.Close();
        }

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
    
    public class HttpTransferStreamIdentity: HttpTransferStream
    {
        long? RemainingContentLength;
        public HttpTransferStreamIdentity(Stream rawStream, long? contentLength): base(rawStream, contentLength)
        {
            RemainingContentLength = contentLength;
        }

        public override int Read(byte[] buffer, int offset, int length)
        {
            if(RemainingContentLength != null && RemainingContentLength <= 0)
            {
                Close();
                return 0;
            }

            int maxLength = (int)Math.Min(length, RemainingContentLength ?? length);
            int actualLength = RawStream.Read(buffer, offset, maxLength);
            
            if(RemainingContentLength != null)
            {
                RemainingContentLength -= actualLength;
            }

            if(actualLength == 0)
            {
                Close();
            }

            return actualLength;
        }
    }

    public class HttpTransferStreamGzip: HttpTransferStream
    {
        GZipStream GZip;
        
        public HttpTransferStreamGzip(Stream rawStream, long? contentLength): base(rawStream, contentLength)
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
            // Todo: count already read bytes from previous Read calls and use it to compute remaining expected length
            int maxLength = (int)Math.Min(length, ContentLength ?? length);
            int actualLength = GZip.Read(buffer, offset, maxLength);
            return actualLength;
        }
    }
    
    public class HttpTransferStreamChunked: HttpTransferStream
    {
        public HttpTransferStreamChunked(Stream rawStream, long? contentLength): base(rawStream, contentLength)
        {
            ContentLength = contentLength;
        }
        
        long CurrentChunkLength = 0;
        long CurrentChunkRemaining = 0;

        bool EndOfDataReached = false;
        
        public override int Read(byte[] buffer, int offset, int length)
        {
            if(EndOfDataReached)
            {
                return 0;
            }

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

                    // Chunk length zero means end of data.
                    if(CurrentChunkLength == 0)
                    {
                        Close();
                        EndOfDataReached = true;
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