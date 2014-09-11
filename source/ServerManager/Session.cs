using Cyber.Core;
using System;
using System.Net.Sockets;
using System.Text;
namespace Cyber.ServerManager
{
	internal class Session
	{
		private Socket mSock;
		private AsyncCallback mReceivedCallback;
		private bool mClosed;
		private byte[] mDataBuffer;
		private string mIP;
		private string mLongIP;
		private int mDisconnections;
		private int mDisconnectionErrors;
		internal int Disconnection
		{
			get
			{
				return this.mDisconnections;
			}
			set
			{
				this.mDisconnections = value;
			}
		}
		internal int DisconnectionError
		{
			get
			{
				return this.mDisconnectionErrors;
			}
			set
			{
				this.mDisconnectionErrors = value;
			}
		}
		internal string GetLongIP
		{
			get
			{
				return this.mSock.RemoteEndPoint.ToString();
			}
		}
		public Session(Socket pSock)
		{
			this.mSock = pSock;
			this.mDataBuffer = new byte[1024];
			this.mReceivedCallback = new AsyncCallback(this.BytesReceived);
			this.mClosed = false;
			Logging.WriteLine("Received connection", ConsoleColor.Gray);
			this.mIP = this.mSock.RemoteEndPoint.ToString().Split(new char[]
			{
				':'
			})[0];
			this.mLongIP = pSock.RemoteEndPoint.ToString();
			this.SendData("authreq");
			this.ContinueListening();
		}
		private void BytesReceived(IAsyncResult pIar)
		{
			try
			{
				int num = this.mSock.EndReceive(pIar);
				try
				{
					byte[] destinationArray = new byte[num];
					Array.Copy(this.mDataBuffer, destinationArray, num);
					string[] array = Encoding.Default.GetString(this.mDataBuffer, 0, num).Split(new char[]
					{
						'|'
					});
					string[] array2 = array;
					for (int i = 0; i < array2.Length; i++)
					{
						string text = array2[i];
						if (!string.IsNullOrEmpty(text))
						{
							string[] array3 = text.Split(new char[]
							{
								':'
							});
							if (array3[0].Length == 0)
							{
								this.Close();
								return;
							}
						}
					}
					this.ContinueListening();
				}
				catch
				{
					this.Close();
				}
			}
			catch
			{
				this.Close();
			}
		}
		private void ContinueListening()
		{
			try
			{
				this.mSock.BeginReceive(this.mDataBuffer, 0, this.mDataBuffer.Length, SocketFlags.None, this.mReceivedCallback, this);
			}
			catch
			{
				this.Close();
			}
		}
		private void SendData(string pData)
		{
			try
			{
				byte[] bytes = Encoding.Default.GetBytes(pData + "|");
				this.mSock.Send(bytes);
			}
			catch
			{
				this.Close();
			}
		}
		internal void SendMessage(string data)
		{
			this.SendData(data);
		}
		internal void Close()
		{
			if (this.mClosed)
			{
				return;
			}
			this.mClosed = true;
			try
			{
				this.mSock.Close();
			}
			catch
			{
			}
			SessionManagement.RemoveSession(this);
			Logging.WriteLine("Reached end of session", ConsoleColor.Gray);
		}
		internal bool GetState()
		{
			bool result;
			try
			{
				result = this.mSock.Connected;
			}
			catch
			{
				result = false;
			}
			return result;
		}
	}
}
