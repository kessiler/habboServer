using Database_Manager.Database.Session_Details;
using Database_Manager.Database.Session_Details.Interfaces;
using MySql.Data.MySqlClient;
using System;
namespace Database_Manager.Database
{
	public class MySqlClient : IDatabaseClient, IDisposable
	{
		private MySqlConnection connection;
		private DatabaseManager dbManager;
		private IQueryAdapter info;
		public MySqlClient(DatabaseManager dbManager)
		{
			this.dbManager = dbManager;
			this.connection = new MySqlConnection(dbManager.getConnectionString());
		}
		public void connect()
		{
			this.connection.Open();
		}
		public void disconnect()
		{
			try
			{
				this.connection.Close();
			}
			catch
			{
			}
		}
		public void Dispose()
		{
			this.info = null;
			this.disconnect();
		}
		internal MySqlCommand getNewCommand()
		{
			return this.connection.CreateCommand();
		}
		public IQueryAdapter getQueryReactor()
		{
			return this.info;
		}
		public MySqlTransaction getTransaction()
		{
			return this.connection.BeginTransaction();
		}
		public bool isAvailable()
		{
			return this.info == null;
		}
		public void prepare()
		{
			this.info = new NormalQueryReactor(this);
		}
		public void reportDone()
		{
			this.Dispose();
		}
		public MySqlCommand createNewCommand()
		{
			return this.connection.CreateCommand();
		}
	}
}
