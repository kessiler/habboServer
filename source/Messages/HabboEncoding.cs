using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cyber.Messages
{
    internal class HabboEncoding
    {
        internal static byte[] EncodeInt16(int v)
        {
            byte[] t = new byte[2];
            t[0] = (byte)(v >> 8);
            t[1] = (byte)v;
            return t;
        }

        internal static byte[] EncodeInt32(int v)
        {
            byte[] t = new byte[4];
            t[0] = (byte)(v >> 24);
            t[1] = (byte)(v >> 16);
            t[2] = (byte)(v >> 8);
            t[3] = (byte)v;
            return t;
        }

        
        internal static byte[] EncodeUInt(uint v)
        {
            byte[] t = new byte[4];
            t[0] = (byte)(v >> 24);
            t[1] = (byte)(v >> 16);
            t[2] = (byte)(v >> 8);
            t[3] = (byte)v;
            return t;
        }

        internal static int DecodeInt32(byte[] v)
        {

            if ((v[0] | v[1] | v[2] | v[3]) < 0)
            {
                return -1;
            }
            return ((v[0] << 24) + (v[1] << 16) + (v[2] << 8) + (v[3]));

        }

        internal static Int16 DecodeInt16(byte[] v)
        {

            if ((v[0] | v[1]) < 0)
            {
                return -1;
            }
            int result = ((v[0] << 8) + (v[1]));
            return (Int16)result;
        }

        public static byte[] BufferEncode(byte[] byte_0, int int_0, int int_1)
        {
            int length = int_0 + int_1;
            if (length > byte_0.Length)
            {
                length = byte_0.Length;
            }
            if (int_1 > byte_0.Length)
            {
                int_1 = byte_0.Length;
            }
            if (int_1 < 0)
            {
                int_1 = 0;
            }
            byte[] buffer = new byte[int_1];
            for (int i = 0; i < int_1; i++)
            {
                buffer[i] = byte_0[int_0++];
            }
            return buffer;
        }
    }
}
