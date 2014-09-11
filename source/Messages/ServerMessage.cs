using System;
using System.IO;
using System.Text;

namespace Cyber.Messages
{
    // Cyber Emulator v2.1
    // Stable and Fast Server Message
    // Credits to Nasty35 for the idea :)

    internal class ServerMessage
    {
        private MemoryStream Buffer;
        internal short Id;

        internal ServerMessage()
        {
        }

        internal ServerMessage(int Header)
        {
            Init(Header);
        }

        internal void Init(int Header)
        {
            Buffer = new MemoryStream();
            Id = (short)Header;
            AppendShort(Header);
        }

        internal void AppendShort(int S)
        {
            Buffer.Write(HabboEncoding.EncodeInt16(S), 0, 2);
        }

        internal void AppendInt32(int i)
        {
            Buffer.Write(HabboEncoding.EncodeInt32(i), 0, 4);
        }

        internal void AppendUInt(uint i)
        {
            Buffer.Write(HabboEncoding.EncodeUInt(i), 0, 4);
        }

        internal void AppendBoolean(bool b)
        {
            AppendByte(b ? 1 : 0);
        }

        internal void AppendString(string str)
        {
            AppendShort(str.Length);
            Buffer.Write(Encoding.Default.GetBytes(str), 0, str.Length);
        }

        internal void AppendByte(int i)
        {
            Buffer.WriteByte((byte)i);
        }

        internal byte[] GetBytes()
        {
            MemoryStream TemporaryStream = new MemoryStream();
            TemporaryStream.Write(HabboEncoding.EncodeInt32((int)Buffer.Length), 0, 4);
            TemporaryStream.Write(Buffer.ToArray(), 0, (int)Buffer.Length);

            return TemporaryStream.ToArray();
        }

    }
}
