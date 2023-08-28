using System;
using System.IO;

namespace LumiSoft.Net
{
    /// <summary>
    /// Stream line reader.
    /// </summary>
    //[Obsolete("Use StreamHelper instead !")]
    public class StreamLineReader
    {
        private readonly Stream streamSource;
        private string encoding = "";
        private const int READ_BUFFER_SIZE = 1024;
        private readonly byte[] buffer = new byte[1024];

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="source">Source stream from where to read data. Reading begins from stream current position.</param>
        public StreamLineReader(Stream source)
        {
            streamSource = source;
        }

        #region method ReadLine

        /// <summary>
        /// Reads byte[] line from stream. NOTE: Returns null if end of stream reached.
        /// </summary>
        /// <returns>Return null if end of stream reached.</returns>
        public byte[] ReadLine()
        {
            // TODO: Allow to buffer source stream reads

            var sourceArray = buffer;
            var posInBuffer = 0;

            var prevByte = streamSource.ReadByte();
            var currentByteInt = streamSource.ReadByte();
            while (prevByte > -1)
            {
                // CRLF line found
                if (prevByte == (byte) '\r' && (byte) currentByteInt == (byte) '\n')
                {
                    var retVal = new byte[posInBuffer];
                    Array.Copy(sourceArray, retVal, posInBuffer);
                    return retVal;
                }
                // LF line found and only LF lines allowed

                if (!CarriageReturnLineFeedLinesOnly && currentByteInt == '\n')
                {
                    var retVal = new byte[posInBuffer + 1];
                    Array.Copy(sourceArray, retVal, posInBuffer + 1);
                    retVal[posInBuffer] = (byte) prevByte;
                    return retVal;
                }

                // Buffer is full, add addition m_ReadBufferSize bytes
                if (posInBuffer == sourceArray.Length)
                {
                    var newBuffer = new byte[sourceArray.Length + READ_BUFFER_SIZE];
                    Array.Copy(sourceArray, newBuffer, sourceArray.Length);
                    sourceArray = newBuffer;
                }

                sourceArray[posInBuffer] = (byte) prevByte;
                posInBuffer++;
                prevByte = currentByteInt;


                // Read next byte
                currentByteInt = streamSource.ReadByte();
            }

            // Line isn't terminated with <CRLF> and has some bytes left, return them.
            if (posInBuffer <= 0) return null;
            {
                var retVal = new byte[posInBuffer];
                Array.Copy(sourceArray, retVal, posInBuffer);
                return retVal;
            }
        }

        #endregion

        #region method ReadLineString

        /// <summary>
        /// Reads string line from stream. String is converted with specified Encoding property from byte[] line. NOTE: Returns null if end of stream reached.
        /// </summary>
        /// <returns></returns>
        public string ReadLineString()
        {
            var line = ReadLine();
            if (line != null)
                return string.IsNullOrEmpty(encoding)
                    ? System.Text.Encoding.Default.GetString(line)
                    : System.Text.Encoding.GetEncoding(encoding).GetString(line);

            return null;
        }

        #endregion

        #region Properties Implementation

        /// <summary>
        /// Gets or sets charset encoding to use for string based methods. Default("") encoding is system default encoding.
        /// </summary>
        public string Encoding
        {
            get => encoding;

            set
            {
                // Check if encoding is valid
                _ = System.Text.Encoding.GetEncoding(value);
                encoding = value;
            }
        }

        /// <summary>
        /// Gets or sets if lines must be CRLF terminated or may be only LF terminated too.
        /// </summary>
        public bool CarriageReturnLineFeedLinesOnly { get; set; } = true;

        #endregion
    }
}