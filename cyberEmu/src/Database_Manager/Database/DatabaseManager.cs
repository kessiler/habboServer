using Database_Manager.Database.Database_Exceptions;
using Database_Manager.Database.Session_Details.Interfaces;
using Database_Manager.Managers.Database;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
namespace Database_Manager.Database
{
	public class DatabaseManager
	{
		private uint beginClientAmount;
		private string connectionString;
		private List<MySqlClient> databaseClients;
		private bool isConnected;
		private uint maxPoolSize;
		private DatabaseServer server;
		private Queue connections;
		public static bool dbEnabled = true;
		public DatabaseManager(uint maxPoolSize, uint clientAmount)
		{
			if (maxPoolSize < clientAmount)
			{
				throw new DatabaseException("The poolsize can not be larger than the client amount!");
			}
			this.beginClientAmount = clientAmount;
			this.maxPoolSize = maxPoolSize;
			this.connections = new Queue();
		}
		private void createNewConnectionString()
		{
			MySqlConnectionStringBuilder mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder();
			mySqlConnectionStringBuilder.Server = this.server.getHost();
			mySqlConnectionStringBuilder.Port = this.server.getPort();
			mySqlConnectionStringBuilder.UserID = this.server.getUsername();
			mySqlConnectionStringBuilder.Password = this.server.getPassword();
            mySqlConnectionStringBuilder.Database = this.server.getDatabaseName();
			mySqlConnectionStringBuilder.MinimumPoolSize = this.beginClientAmount;
            mySqlConnectionStringBuilder.MaximumPoolSize = this.maxPoolSize;
            mySqlConnectionStringBuilder.Pooling = true;
            mySqlConnectionStringBuilder.AllowZeroDateTime = true;
			mySqlConnectionStringBuilder.ConvertZeroDateTime = true;
			mySqlConnectionStringBuilder.DefaultCommandTimeout = 300;
			mySqlConnectionStringBuilder.ConnectionTimeout = 10;
			MySqlConnectionStringBuilder mySqlConnectionStringBuilder2 = mySqlConnectionStringBuilder;
			this.setConnectionString(mySqlConnectionStringBuilder2.ToString());
		}
		public void destroy()
		{
			bool flag = false;
			try
			{
				Monitor.Enter(this, ref flag);
				this.isConnected = false;
				if (this.databaseClients != null)
				{
					foreach (MySqlClient current in this.databaseClients)
					{
						if (!current.isAvailable())
						{
							current.Dispose();
						}
						current.disconnect();
					}
					this.databaseClients.Clear();
				}
			}
			finally
			{
				if (flag)
				{
					Monitor.Exit(this);
				}
			}
		}
		internal string getConnectionString()
		{
			return this.connectionString;
		}
		public IQueryAdapter getQueryreactor()
		{
			IDatabaseClient databaseClient = new MySqlClient(this);
			databaseClient.connect();
			databaseClient.prepare();
			return databaseClient.getQueryReactor();
		}
		internal void FreeConnection(IDatabaseClient dbClient)
		{
			lock (this.connections.SyncRoot)
			{
				this.connections.Enqueue(dbClient);
			}
		}
		public void init()
		{
			try
			{
				this.createNewConnectionString();
				this.databaseClients = new List<MySqlClient>(checked((int)this.maxPoolSize));
			}
			catch (MySqlException ex)
			{
				this.isConnected = false;
				throw new Exception("Could not connect the clients to the database: " + ex.Message);
			}
			this.isConnected = true;
		}
		public bool isConnectedToDatabase()
		{
			return this.isConnected;
		}
		private void setConnectionString(string connectionString)
		{
			this.connectionString = connectionString;
		}
		public bool setServerDetails(string host, uint port, string username, string password, string databaseName)
		{
			bool result;
			try
			{
				this.server = new DatabaseServer(host, port, username, password, databaseName);
				result = true;
			}
			catch (DatabaseException)
			{
				this.isConnected = false;
				result = false;
			}
			return result;
		}
	}
}
