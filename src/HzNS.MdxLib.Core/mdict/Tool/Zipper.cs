using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using HzNS.MdxLib.Compression.impl;
using Ionic.Zlib;

namespace HzNS.MdxLib.MDict.Tool
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class Zipper
    {
        
        private const int BufferSize = 4096;
        private const int LooseBufferSize = 32768;
        
        
        #region zlib (pure) - InflateBufferWithPureZlib
        
        public static byte[] InflateBufferWithPureZlib(byte[] compressedBytes, int length)
        {
            using var buffer = new MemoryStream();
            InflateBufferWithPureZlib(compressedBytes, length, buffer);
            return buffer.ToArray();
        }

        /// <summary>
        /// InflateBufferWithPureZlib
        /// </summary>
        /// <param name="compressedBytes"></param>
        /// <param name="length"></param>
        /// <param name="outs"></param>
        /// <exception cref="Exception"></exception>
        public static void InflateBufferWithPureZlib(byte[] compressedBytes, int length, Stream outs)
        {
            //int bufferSize = 1024;
            var buffer = new byte[BufferSize];
            var decompressor = new ZlibCodec();

            var rc = decompressor.InitializeInflate();

            decompressor.InputBuffer = compressedBytes;
            decompressor.NextIn = 0;
            decompressor.AvailableBytesIn = length;

            decompressor.OutputBuffer = buffer;

            // pass 1: inflate 
            do
            {
                decompressor.NextOut = 0;
                decompressor.AvailableBytesOut = buffer.Length;
                rc = decompressor.Inflate(FlushType.None);

                if (rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END)
                    throw new Exception("inflating: " + decompressor.Message);

                outs.Write(decompressor.OutputBuffer, 0, buffer.Length - decompressor.AvailableBytesOut);
            } while (decompressor.AvailableBytesIn > 0 && decompressor.AvailableBytesOut == 0);

            // pass 2: finish and flush
            do
            {
                decompressor.NextOut = 0;
                decompressor.AvailableBytesOut = buffer.Length;
                rc = decompressor.Inflate(FlushType.Finish);

                if (rc != ZlibConstants.Z_STREAM_END && rc != ZlibConstants.Z_OK)
                    throw new Exception("inflating: " + decompressor.Message);

                if (buffer.Length - decompressor.AvailableBytesOut > 0)
                    outs.Write(buffer, 0, buffer.Length - decompressor.AvailableBytesOut);
            } while (decompressor.AvailableBytesIn > 0 && decompressor.AvailableBytesOut == 0);

            decompressor.EndInflate();
        }

        #endregion

        
        #region MiniLZO Compress

        public static bool MiniLzoCompress(string inName, string outName)
        {
            try
            {
                using var fsIn =
                    new FileStream(inName, FileMode.Open, FileAccess.Read, FileShare.Read, LooseBufferSize);
                return MiniLzoCompress(fsIn, outName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool MiniLzoCompress(Stream fsIn, string outName)
        {
            try
            {
                using var fsOut = File.Create(outName);
                return MiniLzoCompress(fsIn, fsOut);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool MiniLzoCompress(Stream fsIn, Stream fsOut)
        {
            var buf = new byte[LooseBufferSize];
            try
            {
                while (true)
                {
                    var count = fsIn.Read(buf, 0, LooseBufferSize);
                    if (count != 0)
                    {
                        //compressing.Write(buf, 0, count);
                        if (count < LooseBufferSize)
                        {
                            var x = new byte[count];
                            Array.Copy(buf, x, count);
                            // buf = null;
                            buf = x;
                        }

                        MiniLZO.Compress(buf, out var compData);
                        var b = (byte) (compData.Length & 0xff);
                        fsOut.WriteByte(b);
                        b = (byte) ((compData.Length >> 8) & 0xff);
                        fsOut.WriteByte(b);
                        fsOut.Write(compData, 0, compData.Length);
                        // compData = null;
                    }

                    if (count != LooseBufferSize)
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        #endregion

        #region MiniLZO Decompress

        public static bool MiniLzoDecompress(string inName, string outName)
        {
            try
            {
                using var fsIn =
                    new FileStream(inName, FileMode.Open, FileAccess.Read, FileShare.Read, LooseBufferSize);
                return MiniLzoDecompress(fsIn, outName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static bool MiniLzoDecompress(Stream fsIn, string outName)
        {
            try
            {
                using var fsOut = File.Create(outName);
                return MiniLzoDecompress(fsIn, fsOut, out _);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        public static int MiniLzoDecompress(byte[] dataIn, byte[] dataOut)
        {
            using var inStream = new MemoryStream(dataIn, 0, dataIn.Length);
            using var outStream = new MemoryStream(dataOut, 0, dataOut.Length, true);
            MiniLzoDecompress(inStream, outStream, out var decompressedSize);
            return decompressedSize;
        }

        public static bool MiniLzoDecompress(byte[] dataIn, int start, int length, byte[] dataOut)
        {
            using var inStream = new MemoryStream(dataIn, start, length);
            using var outStream = new MemoryStream(dataOut, 0, dataOut.Length, true);
            return MiniLzoDecompress(inStream, outStream, out _);
        }

        public static bool MiniLzoDecompress(Stream fsIn, Stream fsOut, out int decompressedSize)
        {
            //_checkSum = 0;
            var decompressedData = new byte[LooseBufferSize];
            decompressedSize = 0;
            //byte b;

            try
            {
                while (true)
                {
                    var count = (int) fsIn.ReadByte();
                    count += (fsIn.ReadByte() << 8);
                    var buf = new byte[count];
                    count = fsIn.Read(buf, 0, count);
                    if (count != 0)
                    {
                        decompressedSize = MiniLZO.Decompress(buf, decompressedData);
                        //_checkSum += AcedUtils.Adler32(_decompData, 0, _decompData.Length);
                        fsOut.Write(decompressedData ?? throw new Exception("Null decompressedData"),
                            0, decompressedSize);
                        decompressedData = null;
                    }

                    buf = null;

                    if (count != LooseBufferSize)
                        break;
                    if (count == 0)
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            return false;
        }

        #endregion

    }
}