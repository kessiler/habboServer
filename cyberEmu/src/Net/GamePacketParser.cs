using ConnectionManager;
using Cyber.Core;
using Cyber.HabboHotel.GameClients;
using Cyber.Messages;
using Cyber.Messages.ClientMessages;
using SharedPacketLib;
using System;
namespace Cyber.Net
{
	public class GamePacketParser : IDataParser, IDisposable, ICloneable
	{
		public delegate void HandlePacket(ClientMessage message);
		private ConnectionInformation con;
		private GameClient currentClient;
		public event GamePacketParser.HandlePacket onNewPacket;
		internal GamePacketParser(GameClient me)
		{
			this.currentClient = me;
		}
		public void SetConnection(ConnectionInformation con)
		{
			this.con = con;
			this.onNewPacket = null;
		}
		public void handlePacketData(byte[] data)
		{
			int i = 0;
            if (currentClient != null && currentClient.ARC4 != null)
            {
                currentClient.ARC4.Decrypt(ref data);
            }  
			checked
			{
				while (i < data.Length)
				{
					try
					{
						int num = HabboEncoding.DecodeInt32(new byte[]
						{
							data[i++],
							data[i++],
							data[i++],
							data[i++]
						});
						if (num >= 2 && num <= 1024)
						{
							int messageId = HabboEncoding.DecodeInt16(new byte[]
							{
								data[i++],
								data[i++]
							});
							byte[] array = new byte[num - 2];
							int num2 = 0;
							while (num2 < array.Length && i < data.Length)
							{
								array[num2] = data[i++];
								num2++;
							}
							if (this.onNewPacket != null)
							{
								using (ClientMessage clientMessage = ClientMessageFactory.GetClientMessage(messageId, array))
								{
									this.onNewPacket(clientMessage);
								}
							}
						}
					}
					catch (Exception pException)
					{
						HabboEncoding.DecodeInt32(new byte[]
						{
							data[i++],
							data[i++],
							data[i++],
							data[i++]
						});
						int num3 = HabboEncoding.DecodeInt16(new byte[]
						{
							data[i++],
							data[i++]
						});
						Logging.HandleException(pException, "packet handling ----> " + num3);
						this.con.Dispose();
					}
				}
			}
		}
		public void Dispose()
		{
			this.onNewPacket = null;
		}
		public object Clone()
		{
			return new GamePacketParser(this.currentClient);
		}
	}
}
