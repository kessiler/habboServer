using ConnectionManager.LoggingSystem;
using ConnectionManager.Messages;
using SharedPacketLib;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Cyber.HabboHotel.GameClients;
using Cyber.Core;

namespace ConnectionManager
{
	public class ConnectionInformation : IDisposable
	{
		public delegate void ConnectionChange(ConnectionInformation information, ConnectionState state);
		private Socket dataSocket;
		private SocketManager manager;
		private string ip;
		private int connectionID;
		private bool isConnected;
		private byte[] buffer;
		private AsyncCallback sendCallback;
		public static bool disableSend;
		public static bool disableReceive;
		public event ConnectionInformation.ConnectionChange connectionChanged;
		public IDataParser parser
		{
			get;
			set;
		}
        private GameClient Client;

		public ConnectionInformation(Socket dataStream, int connectionID, SocketManager manager, IDataParser parser, string ip)
		{
			this.parser = parser;
			this.buffer = new byte[GameSocketManagerStatics.BUFFER_SIZE];
			this.manager = manager;
			this.dataSocket = dataStream;
			this.dataSocket.SendBufferSize = GameSocketManagerStatics.BUFFER_SIZE;
			this.ip = ip;
			this.connectionID = connectionID;
			this.sendCallback = new AsyncCallback(this.sentData);
			if (this.connectionChanged != null)
			{
				this.connectionChanged(this, ConnectionState.open);
			}
			MessageLoggerManager.AddMessage(null, connectionID, LogState.ConnectionOpen);
		}
        public void SetClient(GameClient Client)
        {
            this.Client = Client;
        }
		public void startPacketProcessing()
		{
			if (!this.isConnected)
			{
				this.isConnected = true;
				try
				{
					this.dataSocket.BeginReceive(this.buffer, 0, this.buffer.Length, SocketFlags.None, new AsyncCallback(this.incomingDataPacket), this.dataSocket);
				}
				catch
				{
					this.disconnect();
				}
			}
		}
		public string getIp()
		{
			return this.ip;
		}
		public int getConnectionID()
		{
			return this.connectionID;
		}
		internal void disconnect()
		{
			try
			{
				if (this.isConnected)
				{
					this.isConnected = false;
					MessageLoggerManager.AddMessage(null, this.connectionID, LogState.ConnectionClose);
					try
					{
						if (this.dataSocket != null && this.dataSocket.Connected)
						{
							this.dataSocket.Shutdown(SocketShutdown.Both);
							this.dataSocket.Close();
						}
					}
					catch
					{
					}
					this.dataSocket.Dispose();
					this.parser.Dispose();
					try
					{
						if (this.connectionChanged != null)
						{
							this.connectionChanged(this, ConnectionState.closed);
						}
					}
					catch
					{
					}
					this.connectionChanged = null;
				}
			}
			catch
			{
			}
		}
		public void Dispose()
		{
			if (this.isConnected)
			{
				this.disconnect();
			}
		}
		private void incomingDataPacket(IAsyncResult iAr)
		{
			int num;
			try
			{
				num = this.dataSocket.EndReceive(iAr);
			}
			catch
			{
				this.disconnect();
				return;
			}
			if (num != 0)
			{
				try
				{
					if (!ConnectionInformation.disableReceive)
					{
						byte[] array = new byte[num];
						Array.Copy(this.buffer, array, num);
						MessageLoggerManager.AddMessage(array, this.connectionID, LogState.Normal);
						this.handlePacketData(array);
					}
				}
				catch
				{
					this.disconnect();
				}
				finally
				{
					try
					{
						this.dataSocket.BeginReceive(this.buffer, 0, this.buffer.Length, SocketFlags.None, new AsyncCallback(this.incomingDataPacket), this.dataSocket);
					}
					catch
					{
						this.disconnect();
					}
				}
				return;
			}
			this.disconnect();
		}
		private void handlePacketData(byte[] packet)
		{
			if (this.parser != null)
			{
				this.parser.handlePacketData(packet);
			}
		}
		public void SendData(byte[] packet)
		{
			this.sendData(packet);
		}
		public void SendMuchData(byte[] packet)
		{
			this.sendData(packet);
		}
		public void sendbData(byte[] packet)
		{
			this.sendData(packet);
		}
		private void sendData(byte[] packet)
		{
			try
			{
				this.SendUnsafeData(packet);
			}
			catch
			{
				this.disconnect();
			}
		}
        public void SendUnsafeData(byte[] packet)
        {
            if (!this.isConnected || ConnectionInformation.disableSend)
            {
                return;
            }
            this.dataSocket.BeginSend(packet, 0, packet.Length, SocketFlags.None, this.sendCallback, null);
        }
		private void sentData(IAsyncResult iAr)
		{
			try
			{
				this.dataSocket.EndSend(iAr);
			}
			catch
			{
				this.disconnect();
			}
		}
		private void LogMessage(string message)
		{
			try
			{
				FileStream fileStream = new FileStream("packetlog.txt", FileMode.Append, FileAccess.Write);
				byte[] bytes = Encoding.ASCII.GetBytes(Environment.NewLine + message);
				fileStream.Write(bytes, 0, bytes.Length);
				fileStream.Dispose();
			}
			catch
			{
				Console.WriteLine("UNABLE TO WRITE TO LOGFILE");
			}
		}
	}
}
