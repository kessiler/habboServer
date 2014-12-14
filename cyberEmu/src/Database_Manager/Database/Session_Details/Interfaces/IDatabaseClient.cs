using MySql.Data.MySqlClient;
using System;
namespace Database_Manager.Database.Session_Details.Interfaces
{
	internal interface IDatabaseClient : IDisposable
	{
		void connect();
		void disconnect();
		IQueryAdapter getQueryReactor();
		MySqlCommand createNewCommand();
		MySqlTransaction getTransaction();
		bool isAvailable();
		void prepare();
		void reportDone();
	}
}
