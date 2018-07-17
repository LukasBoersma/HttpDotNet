using System.IO;

namespace HttpDotNet
{
    public static class StreamHelpers
    {
        public static byte[] ReadAllBytes(this BinaryReader reader)
        {
            using (var ms = new MemoryStream())
            {
                reader.BaseStream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}