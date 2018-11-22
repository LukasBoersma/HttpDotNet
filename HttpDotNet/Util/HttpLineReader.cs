using System;
using System.IO;
using System.Text;

namespace HttpDotNet
{
    public static class HttpLineReader
    {
        static byte[] buffer = new byte[4096];
        public static string ReadLine(Stream rawStream)
        {
            long totalBytesRead = 0;
            var line = new StringBuilder();
            bool foundLineEnd = false;
            bool foundFirstPartOfLineEnd = false;
            while(!foundLineEnd)
            {
                int byteCountInBuffer = 0;
                for(int bufferOffset = 0; bufferOffset < buffer.Length; ++bufferOffset)
                {
                    var actualBytesRead = rawStream.Read(buffer, bufferOffset, 1);
                    totalBytesRead += actualBytesRead;
                    
                    if(actualBytesRead == 0)
                    {
                        if(totalBytesRead == 0)
                        {
                            return null;
                        }
                        else
                        {
                            foundLineEnd = true;
                            break;
                        }
                    }
                    
                    byteCountInBuffer += actualBytesRead;
                    

                    if(buffer[bufferOffset] == '\r')
                    {
                        foundFirstPartOfLineEnd = true;
                    }
                    else if(foundFirstPartOfLineEnd && buffer[bufferOffset] == '\n')
                    {
                        foundLineEnd = true;
                        
                        // Don't include the line end bytes in the returned string
                        byteCountInBuffer -= 2;
                        break;
                    }
                    else
                    {
                        foundFirstPartOfLineEnd = false;
                    }
                }
                
                // Because the raw HTTP lines are only allowed to contain ASCII characters,
                // we can convert any incomplete byte range into strings without worrying 
                // about cutting through the middle of encoded characters.
                line.Append(Encoding.ASCII.GetString(buffer, 0, byteCountInBuffer));
            }
            return line.ToString();
        }
    }
}