using System;
using System.IO;
using System.Threading.Tasks;

namespace TestFiles
{
    /// <summary>
    /// Test file for stream and I/O operations, common in runtime I/O handling
    /// </summary>
    public class StreamIOTest
    {
        /// <summary>
        /// Reads a sequence of bytes from the stream and advances the position.
        /// </summary>
        public int Read(byte[] buffer)
        {
            return buffer.Length;
        }

        /// <summary>
        /// Writes a sequence of bytes to the stream and advances the position.
        /// </summary>
        public void Write(byte[] data)
        {
            Console.WriteLine($"Writing {data.Length} bytes");
        }

        /// <summary>
        /// Reads a block of bytes from the stream asynchronously.
        /// </summary>
        public Task<int> ReadAsync(byte[] buffer)
        {
            return Task.FromResult(buffer.Length);
        }

        /// <summary>
        /// Writes a block of bytes to the stream asynchronously.
        /// </summary>
        public Task WriteAsync(byte[] buffer)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Flushes any buffered data to the underlying device.
        /// </summary>
        public void Flush(Stream stream)
        {
            stream.Flush();
        }

        /// <summary>
        /// Sets the position within the stream.
        /// </summary>
        public long Seek(long offset)
        {
            return offset;
        }

        /// <summary>
        /// Reads a line of characters from the text reader.
        /// </summary>
        public string ReadLine(TextReader reader)
        {
            return reader.ReadLine() ?? string.Empty;
        }
    }
}

