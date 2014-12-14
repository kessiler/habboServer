using ConnectionManager.Socket_Exceptions;
using SharedPacketLib;
using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Collections.Specialized;

namespace ConnectionManager
{
	public class SocketManager
	{
		public delegate void ConnectionEvent(ConnectionInformation connection);
		private HybridDictionary activeConnections;
		private HybridDictionary ipConnectionCount;
		private int maxIpConnectionCount;
		private int acceptedConnections;
		private Socket connectionListener;
		private int portInformation;
		private int maximumConnections;
		private bool acceptConnections;
		private IDataParser parser;
		private bool disableNagleAlgorithm;
		public event SocketManager.ConnectionEvent connectionEvent;
		public void init(int portID, int maxConnections, int connectionsPerIP, IDataParser parser, bool disableNaglesAlgorithm)
		{
			this.parser = parser;
			this.disableNagleAlgorithm = disableNaglesAlgorithm;
			this.initializeFields();
			this.maximumConnections = maxConnections;
			this.portInformation = portID;
			this.maxIpConnectionCount = connectionsPerIP;
			this.acceptedConnections = 0;
			this.prepareConnectionDetails();
		}
		private void initializeFields()
		{
			this.activeConnections = new HybridDictionary();
			this.ipConnectionCount = new HybridDictionary();
		}
		private void prepareConnectionDetails()
		{
			this.connectionListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.connectionListener.NoDelay = this.disableNagleAlgorithm;
			try
			{
				this.connectionListener.Bind(new IPEndPoint(IPAddress.Any, this.portInformation));
			}
			catch (SocketException ex)
			{
				throw new SocketInitializationException(ex.Message);
			}
		}
		public void initializeConnectionRequests()
		{
			this.connectionListener.Listen(100);
			this.acceptConnections = true;
			string hostName = Dns.GetHostName();
			Dns.GetHostEntry(hostName);
			try
			{
				this.connectionListener.BeginAccept(new AsyncCallback(this.newConnectionRequest), this.connectionListener);
			}
			catch
			{
				this.destroy();
			}
		}
		public void destroy()
		{
			this.acceptConnections = false;
			try
			{
				this.connectionListener.Close();
			}
			catch
			{
			}
			this.connectionListener = null;
		}
		private void newConnectionRequest(IAsyncResult iAr)
		{
			checked
			{
				if (this.connectionListener != null && this.acceptConnections)
				{
					try
					{
						Socket socket = ((Socket)iAr.AsyncState).EndAccept(iAr);
						socket.NoDelay = this.disableNagleAlgorithm;
						string ip = socket.RemoteEndPoint.ToString().Split(new char[]
						{
							':'
						})[0];
						this.acceptedConnections++;
						ConnectionInformation connectionInformation = new ConnectionInformation(socket, this.acceptedConnections, this, this.parser.Clone() as IDataParser, ip);
						this.reportUserLogin(ip);
						connectionInformation.connectionChanged += new ConnectionInformation.ConnectionChange(this.c_connectionChanged);
						if (this.connectionEvent != null)
						{
							this.connectionEvent(connectionInformation);
						}
					}
					catch
					{
					}
					finally
					{
						this.connectionListener.BeginAccept(new AsyncCallback(this.newConnectionRequest), this.connectionListener);
					}
				}
			}
		}
		private void c_connectionChanged(ConnectionInformation information, ConnectionState state)
		{
			if (state == ConnectionState.closed)
			{
				this.reportDisconnect(information);
			}
		}
		public void reportDisconnect(ConnectionInformation gameConnection)
		{
			gameConnection.connectionChanged -= new ConnectionInformation.ConnectionChange(this.c_connectionChanged);
			this.reportUserLogout(gameConnection.getIp());
		}
		private void reportUserLogin(string ip)
		{
			this.alterIpConnectionCount(ip, checked(this.getAmountOfConnectionFromIp(ip) + 1));
		}
		private void reportUserLogout(string ip)
		{
			this.alterIpConnectionCount(ip, checked(this.getAmountOfConnectionFromIp(ip) - 1));
		}
		private void alterIpConnectionCount(string ip, int amount)
		{
		}
		private int getAmountOfConnectionFromIp(string ip)
		{
			return 0;
		}
		internal bool isConnected()
		{
			return this.connectionListener != null;
		}
		public int getAcceptedConnections()
		{
			return this.acceptedConnections;
		}
	}
}
