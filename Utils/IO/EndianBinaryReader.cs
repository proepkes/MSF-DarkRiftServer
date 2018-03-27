using System;
using System.IO;
using System.Text;
using Utils.Conversion;

namespace Utils.IO
{
    /// <summary>
    ///     Equivalent of System.IO.BinaryReader, but with either endianness, depending on
    ///     the EndianBitConverter it is constructed with. No data is buffered in the
    ///     e.Reader; the client may seek within the stream at will.
    /// </summary>
    public class EndianBinaryReader : IDisposable
    {
        #region IDisposable Members

        /// <summary>
        ///     Disposes of the underlying stream.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;
                ((IDisposable) BaseStream).Dispose();
            }
        }

        #endregion

        #region Fields not directly related to properties

        /// <summary>
        ///     Whether or not this e.Reader has been disposed yet.
        /// </summary>
        private bool disposed;

        /// <summary>
        ///     Buffer used for temporary storage before conversion into primitives
        /// </summary>
        private readonly byte[] buffer = new byte[16];

        /// <summary>
        ///     Minimum number of bytes used to encode a character
        /// </summary>
        private readonly int minBytesPerChar;

        #endregion

        #region Constructors

        /// <summary>
        ///     Equivalent of System.IO.BinaryWriter, but with either endianness, depending on
        ///     the EndianBitConverter it is constructed with.
        /// </summary>
        /// <param name="bitConverter">Converter to use when reading data</param>
        /// <param name="stream">Stream to read data from</param>
        public EndianBinaryReader(EndianBitConverter bitConverter,
            Stream stream) : this(bitConverter, stream, Encoding.UTF8)
        {
        }

        /// <summary>
        ///     Constructs a new binary e.Reader with the given bit converter, reading
        ///     to the given stream, using the given encoding.
        /// </summary>
        /// <param name="bitConverter">Converter to use when reading data</param>
        /// <param name="stream">Stream to read data from</param>
        /// <param name="encoding">Encoding to use when reading character data</param>
        public EndianBinaryReader(EndianBitConverter bitConverter, Stream stream, Encoding encoding)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (!stream.CanRead)
                throw new ArgumentException("Stream isn't writable", "stream");
            BaseStream = stream;
            BitConverter = bitConverter ?? throw new ArgumentNullException("bitConverter");
            Encoding = encoding ?? throw new ArgumentNullException("encoding");
            minBytesPerChar = 1;

            if (encoding is UnicodeEncoding)
                minBytesPerChar = 2;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     The bit converter used to read values from the stream
        /// </summary>
        public EndianBitConverter BitConverter { get; }

        /// <summary>
        ///     The encoding used to read strings
        /// </summary>
        public Encoding Encoding { get; }

        /// <summary>
        ///     Gets the underlying stream of the EndianBinaryReader.
        /// </summary>
        public Stream BaseStream { get; }

        #endregion

        #region Public methods

        /// <summary>
        ///     Reads a 16-bit signed integer from the stream, using the bit converter
        ///     for this e.Reader. 2 bytes are read.
        /// </summary>
        /// <returns>The 16-bit integer read</returns>
        public short ReadInt16()
        {
            ReadInternal(buffer, 2);
            return BitConverter.ToInt16(buffer, 0);
        }

        /// <summary>
        ///     Reads a 32-bit signed integer from the stream, using the bit converter
        ///     for this e.Reader. 4 bytes are read.
        /// </summary>
        /// <returns>The 32-bit integer read</returns>
        public int ReadInt32()
        {
            ReadInternal(buffer, 4);
            return BitConverter.ToInt32(buffer, 0);
        }

        /// <summary>
        ///     Reads the specified number of bytes, returning them in a new byte array.
        ///     If not enough bytes are available before the end of the stream, this
        ///     method will return what is available.
        /// </summary>
        /// <param name="count">The number of bytes to read</param>
        /// <returns>The bytes read</returns>
        public byte[] ReadBytes(int count)
        {
            CheckDisposed();
            if (count < 0)
                throw new ArgumentOutOfRangeException("count");
            var ret = new byte[count];
            var index = 0;
            while (index < count)
            {
                var read = BaseStream.Read(ret, index, count - index);
                // Stream has finished half way through. That's fine, return what we've got.
                if (read == 0)
                {
                    var copy = new byte[index];
                    Buffer.BlockCopy(ret, 0, copy, 0, index);
                    return copy;
                }

                index += read;
            }

            return ret;
        }
        /// <summary>
        ///     Reads a length-prefixed string from the stream, using the encoding for this e.Reader.
        ///     A 7-bit encoded integer is first read, which specifies the number of bytes
        ///     to read from the stream. These bytes are then converted into a string with
        ///     the encoding for this e.Reader.
        /// </summary>
        /// <returns>The string read from the stream.</returns>
        public string ReadString()
        {
            var bytesToRead = ReadInt16();

            var data = new byte[bytesToRead];
            ReadInternal(data, bytesToRead);
            return Encoding.GetString(data, 0, data.Length);
        }

        #endregion

        #region Private methods

        /// <summary>
        ///     Checks whether or not the e.Reader has been disposed, throwing an exception if so.
        /// </summary>
        private void CheckDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException("EndianBinaryReader");
        }

        /// <summary>
        ///     Reads the given number of bytes from the stream, throwing an exception
        ///     if they can't all be read.
        /// </summary>
        /// <param name="data">Buffer to read into</param>
        /// <param name="size">Number of bytes to read</param>
        private void ReadInternal(byte[] data, int size)
        {
            CheckDisposed();
            var index = 0;
            while (index < size)
            {
                var read = BaseStream.Read(data, index, size - index);
                if (read == 0)
                    throw new EndOfStreamException
                    (string.Format("End of stream reached with {0} byte{1} left to read.", size - index,
                        size - index == 1 ? "s" : ""));
                index += read;
            }
        }

        /// <summary>
        ///     Reads the given number of bytes from the stream if possible, returning
        ///     the number of bytes actually read, which may be less than requested if
        ///     (and only if) the end of the stream is reached.
        /// </summary>
        /// <param name="data">Buffer to read into</param>
        /// <param name="size">Number of bytes to read</param>
        /// <returns>Number of bytes actually read</returns>
        private int TryReadInternal(byte[] data, int size)
        {
            CheckDisposed();
            var index = 0;
            while (index < size)
            {
                var read = BaseStream.Read(data, index, size - index);
                if (read == 0)
                    return index;
                index += read;
            }

            return index;
        }

        #endregion
    }
}