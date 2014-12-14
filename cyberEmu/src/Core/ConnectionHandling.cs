using ConnectionManager;
using Cyber.Net;
using System;
namespace Cyber.Core
{
	public class ConnectionHandling
	{
		private SocketManager manager;
		public ConnectionHandling(int port, int maxConnections, int connectionsPerIP, bool enabeNagles)
		{
			this.manager = new SocketManager();
			this.manager.init(port, maxConnections, connectionsPerIP, new InitialPacketParser(), !enabeNagles);
		}
		internal void init()
		{
			this.manager.connectionEvent += new SocketManager.ConnectionEvent(this.manager_connectionEvent);
		}
		private void manager_connectionEvent(ConnectionInformation connection)
		{
			connection.connectionChanged += new ConnectionInformation.ConnectionChange(this.connectionChanged);
			CyberEnvironment.GetGame().GetClientManager().CreateAndStartClient(checked((uint)connection.getConnectionID()), connection);
		}
		private void connectionChanged(ConnectionInformation information, ConnectionState state)
		{
			if (state == ConnectionState.closed)
			{
				this.CloseConnection(information);
			}
		}
		internal void Start()
		{
			this.manager.initializeConnectionRequests();
		}
		private void CloseConnection(ConnectionInformation Connection)
		{
			try
			{
				Connection.Dispose();
				CyberEnvironment.GetGame().GetClientManager().DisposeConnection(checked((uint)Connection.getConnectionID()));
			}
			catch (Exception ex)
			{
				Logging.LogException(ex.ToString());
			}
		}
		internal void Destroy()
		{
			this.manager.destroy();
		}
	}
}
