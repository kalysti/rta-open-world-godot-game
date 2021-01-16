using  ZeroFormatter;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System;
using System.IO.Compression;

namespace Game.Networking
{
    public class NetworkCompressor
    {
        internal static string Compress(object obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, obj);
                return CompressString(Convert.ToBase64String(stream.ToArray()));
            }
        }

        internal static T Decompress<T>(string obj)
        {
            byte[] bytes = Convert.FromBase64String(DecompressString(obj));

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                return (T) new BinaryFormatter().Deserialize(stream);
            }
        }

          internal static string CompressString(string uncompressedString)
    {
        byte[] compressedBytes;

        using (var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(uncompressedString)))
        {
            using (var compressedStream = new MemoryStream())
            { 
                // setting the leaveOpen parameter to true to ensure that compressedStream will not be closed when compressorStream is disposed
                // this allows compressorStream to close and flush its buffers to compressedStream and guarantees that compressedStream.ToArray() can be called afterward
                // although MSDN documentation states that ToArray() can be called on a closed MemoryStream, I don't want to rely on that very odd behavior should it ever change
                using (var compressorStream = new DeflateStream(compressedStream, CompressionLevel.Fastest, true))
                {
                    uncompressedStream.CopyTo(compressorStream);
                }

                // call compressedStream.ToArray() after the enclosing DeflateStream has closed and flushed its buffer to compressedStream
                compressedBytes = compressedStream.ToArray();
            }
        }

        return Convert.ToBase64String(compressedBytes);
    }


        internal static string DecompressString(string compressedString)
        {
            byte[] decompressedBytes;

            var compressedStream = new MemoryStream(Convert.FromBase64String(compressedString));

            using (var decompressorStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
            {
                using (var decompressedStream = new MemoryStream())
                {
                    decompressorStream.CopyTo(decompressedStream);

                    decompressedBytes = decompressedStream.ToArray();
                }
            }

            return Encoding.UTF8.GetString(decompressedBytes);
        }
    }


}

