using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace NSMB.Utils
{
    public static class SerializationUtils
    {
        #region INT PRECISION

        public static void PackToInt(List<byte> buffer, Vector2 input, float xMin, float xMax, float? yMin = null,
            float? yMax = null)
        {
            SetIfNull(ref yMin, xMin);
            SetIfNull(ref yMax, xMax);

            PackToShort(buffer, input.x, xMin, xMax);
            PackToShort(buffer, input.y, (float)yMin, (float)yMax);
        }

        public static void UnpackFromInt(List<byte> buffer, ref int index, float xMin, float xMax, out Vector2 output,
            float? yMin = null, float? yMax = null)
        {
            SetIfNull(ref yMin, xMin);
            SetIfNull(ref yMax, xMax);

            UnpackFromShort(buffer, ref index, xMin, xMax, out var x);
            UnpackFromShort(buffer, ref index, (float)yMin, (float)yMax, out var y);

            output = new Vector2(x, y);
        }

        #endregion

        #region SHORT PRECISION

        public static void PackToShort(List<byte> buffer, float input, float min, float max)
        {
            var range = max - min;
            var shortValue = (short)((input - min) / range * short.MaxValue);
            WriteShort(buffer, shortValue);
        }

        public static void UnpackFromShort(List<byte> buffer, ref int index, float min, float max, out float output)
        {
            var range = max - min;
            ReadShort(buffer, ref index, out ushort shortValue);
            output = (float)shortValue / ushort.MaxValue * range * 2f + min;
        }

        public static void UnpackFromShort(List<byte> buffer, ref int index, float xMin, float xMax, out Vector2 output,
            float? yMin = null, float? yMax = null)
        {
            SetIfNull(ref yMin, xMin);
            SetIfNull(ref yMax, xMax);

            UnpackFromByte(buffer, ref index, xMin, xMax, out var x);
            UnpackFromByte(buffer, ref index, (float)yMin, (float)yMax, out var y);

            output = new Vector2(x, y);
        }

        public static void PackToShort(List<byte> buffer, Vector2 input, float xMin, float xMax, float? yMin = null,
            float? yMax = null)
        {
            SetIfNull(ref yMin, xMin);
            SetIfNull(ref yMax, xMax);

            PackToByte(buffer, input.x, xMin, xMax);
            PackToByte(buffer, input.y, (float)yMin, (float)yMax);
        }

        public static void PackToShort(List<byte> buffer, params bool[] flags)
        {
            PackToShort(out var shortValue, flags);
            WriteShort(buffer, shortValue);
        }

        public static void PackToShort(out short output, params bool[] flags)
        {
            output = 0;
            for (var i = 0; i < flags.Length; i++)
                output |= (short)((flags[i] ? 1 : 0) << i);
        }

        public static void UnpackFromShort(List<byte> buffer, ref int index, out bool[] output)
        {
            ReadShort(buffer, ref index, out short shortValue);
            output = new bool[16];
            for (var i = 0; i < 16; i++)
                output[i] = Utils.BitTest(shortValue, i);
        }

        #endregion

        #region BYTE PRECISION

        public static void PackToByte(List<byte> buffer, float input, float min, float max)
        {
            var range = max - min;
            var byteValue = (byte)((input - min) / range * byte.MaxValue);
            WriteByte(buffer, byteValue);
        }

        public static void UnpackFromByte(List<byte> buffer, ref int index, float min, float max, out float output)
        {
            var range = max - min;
            ReadByte(buffer, ref index, out var byteValue);
            output = (float)byteValue / byte.MaxValue * range + min;
        }

        #endregion

        #region FLAGS

        public static void PackToByte(List<byte> buffer, params bool[] flags)
        {
            PackToShort(out var byteValue, flags);
            WriteShort(buffer, byteValue);
        }

        public static void PackToByte(out byte output, params bool[] flags)
        {
            output = 0;
            for (var i = 0; i < flags.Length; i++)
                output |= (byte)((flags[i] ? 1 : 0) << i);
        }

        public static void UnpackFromByte(List<byte> buffer, ref int index, out bool[] output)
        {
            ReadByte(buffer, ref index, out var byteOut);
            UnpackFromByte(byteOut, out output);
        }

        public static void UnpackFromByte(byte input, out bool[] output)
        {
            output = new bool[8];
            for (var i = 0; i < 8; i++)
                output[i] = Utils.BitTest(input, i);
        }

        #endregion

        #region Helper Methods

        public static void WriteInt(List<byte> buffer, int input)
        {
            WriteShort(buffer, (short)(input >> 16));
            WriteShort(buffer, (short)(input >> 0));
        }

        public static void WriteShort(List<byte> buffer, short input)
        {
            buffer.Add((byte)(input >> 8));
            buffer.Add((byte)input);
        }

        /// <summary>
        ///     Redundant for buffer.Add(byte);
        /// </summary>
        public static void WriteByte(List<byte> buffer, byte input)
        {
            buffer.Add(input);
        }

        public static void ReadInt(List<byte> buffer, ref int index, out uint value)
        {
            ReadShort(buffer, ref index, out ushort high);
            ReadShort(buffer, ref index, out ushort low);

            value = 0;
            value |= (uint)high << 16;
            value |= low;
        }

        public static void ReadInt(List<byte> buffer, ref int index, out int value)
        {
            ReadInt(buffer, ref index, out uint uValue);
            value = unchecked((int)uValue);
        }

        public static void ReadShort(List<byte> buffer, ref int index, out ushort value)
        {
            ReadByte(buffer, ref index, out var high);
            ReadByte(buffer, ref index, out var low);

            value = 0;
            value |= (ushort)(high << 8);
            value |= low;
        }

        public static void ReadShort(List<byte> buffer, ref int index, out short value)
        {
            ReadShort(buffer, ref index, out ushort uValue);
            value = unchecked((short)uValue);
        }

        public static void ReadByte(List<byte> buffer, ref int index, out byte value)
        {
            value = buffer[index++];
        }

        private static void SetIfNull<T>(ref T checkValue, T setValue)
        {
            if (checkValue == null)
                checkValue = setValue;
        }

        #endregion

        #region Compression

        public static byte[] Compress(byte[] input)
        {
            MemoryStream ms = new();
            DeflateStream ds = new(ms, CompressionMode.Compress);

            ds.Write(input, 0, input.Length);

            ds.Close();
            ms.Close();

            return ms.ToArray();
        }

        public static byte[] Decompress(byte[] input)
        {
            MemoryStream decompressed = new();
            MemoryStream compressed = new(input);
            DeflateStream ds = new(compressed, CompressionMode.Decompress);

            ds.CopyTo(decompressed);

            decompressed.Close();
            compressed.Close();
            ds.Close();

            return decompressed.ToArray();
        }

        #endregion
    }
}