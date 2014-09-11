using Cyber.Messages.ClientMessages;
using System;
using System.Text;
namespace Cyber.Messages
{
	public class ClientMessage : IDisposable
	{
		private int MessageId;
		private byte[] Body;
		private int Pointer;
		internal int Id
		{
			get
			{
				return this.MessageId;
			}
		}
		internal int RemainingLength
		{
			get
			{
				return checked(this.Body.Length - this.Pointer);
			}
		}
		internal int Header
		{
			get
			{
				return this.MessageId;
			}
		}
		internal ClientMessage(int messageID, byte[] body)
		{
			this.Init(messageID, body);
		}
		internal void Init(int messageID, byte[] body)
		{
			if (body == null)
			{
				body = new byte[0];
			}
			this.MessageId = messageID;
			this.Body = body;
			this.Pointer = 0;
		}
		
		internal void AdvancePointer(int i)
		{
			checked
			{
				this.Pointer += i * 4;
			}
		}
		internal byte[] ReadBytes(int Bytes)
		{
			if (Bytes > this.RemainingLength)
			{
				Bytes = this.RemainingLength;
			}
			byte[] array = new byte[Bytes];
			checked
			{
				for (int i = 0; i < Bytes; i++)
				{
					array[i] = this.Body[unchecked(this.Pointer++)];
				}
				return array;
			}
		}
		internal byte[] PlainReadBytes(int Bytes)
		{
			if (Bytes > this.RemainingLength)
			{
				Bytes = this.RemainingLength;
			}
			byte[] array = new byte[Bytes];
			int i = 0;
			int num = this.Pointer;
			checked
			{
				while (i < Bytes)
				{
					array[i] = this.Body[num];
					i++;
					num++;
				}
				return array;
			}
		}
		internal byte[] ReadFixedValue()
		{
			int bytes = HabboEncoding.DecodeInt16(this.ReadBytes(2));
			return this.ReadBytes(bytes);
		}
		internal string PopFixedString()
		{
			return this.PopFixedString(CyberEnvironment.GetDefaultEncoding());
		}
		internal string PopFixedString(Encoding encoding)
		{
			return encoding.GetString(this.ReadFixedValue());
		}
		internal int PopFixedInt32()
		{
			int result = 0;
			string s = this.PopFixedString(Encoding.ASCII);
			int.TryParse(s, out result);
			return result;
		}
		internal bool PopWiredBoolean()
		{
			return this.RemainingLength > 0 && (char)this.Body[this.Pointer++] == Convert.ToChar(1);
		}
		internal int PopWiredInt32()
		{
			if (this.RemainingLength < 1)
			{
				return 0;
			}
			byte[] v = this.PlainReadBytes(4);
			int result = HabboEncoding.DecodeInt32(v);
			checked
			{
				this.Pointer += 4;
				return result;
			}
		}
		internal uint PopWiredUInt()
		{
			return uint.Parse(this.PopWiredInt32().ToString());
		}
		public void Dispose()
		{
			ClientMessageFactory.ObjectCallback(this);
			GC.SuppressFinalize(this);
		}

        public override string ToString()
        {
            string str = " (" + MessageId + ") ";
            str += Encoding.Default.GetString(this.Body);
            for (int i = 0; i < 13; i++)
            {
                str = str.Replace(char.ToString(Convert.ToChar(i)), "[" + i + "]");
            }
            return str;
        }
	}
}
