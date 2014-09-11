using ConnectionManager;
using System;
using System.Collections.Generic;
namespace Cyber.Messages
{
	public class QueuedServerMessage
	{
		private List<byte> packet;
		private ConnectionInformation userConnection;
		internal byte[] getPacket
		{
			get
			{
				return this.packet.ToArray();
			}
		}
		public QueuedServerMessage(ConnectionInformation connection)
		{
			this.userConnection = connection;
			this.packet = new List<byte>();
		}
		internal void Dispose()
		{
			this.packet.Clear();
			this.userConnection = null;
		}
		private void appendBytes(byte[] bytes)
		{
			this.packet.AddRange(bytes);
		}
		internal void appendResponse(ServerMessage message)
		{
			this.appendBytes(message.GetBytes());
		}
		internal void addBytes(byte[] bytes)
		{
			this.appendBytes(bytes);
		}
		internal void sendResponse()
		{
			if (this.userConnection != null)
			{
				this.userConnection.SendMuchData(this.packet.ToArray());
			}
			this.Dispose();
		}
	}
}
