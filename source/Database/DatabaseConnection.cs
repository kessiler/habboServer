using Database_Manager.Database.Session_Details;
using Database_Manager.Database.Session_Details.Interfaces;
using MySql.Data.MySqlClient;
using System;
using System.Data;
namespace Cyber.Database
{
    internal class DatabaseConnection : IDatabaseClient, IDisposable
    {
        private readonly MySqlConnection _mysqlConnection;
        private readonly IQueryAdapter _adapter;
        public DatabaseConnection(string ConnectionStr)
        {
            this._mysqlConnection = new MySqlConnection(ConnectionStr);
            this._adapter = new NormalQueryReactor(this);
        }
        public void Open()
        {
            if (this._mysqlConnection.State == ConnectionState.Closed)
            {
                this._mysqlConnection.Open();
            }
        }
        public void Close()
        {
            if (this._mysqlConnection.State == ConnectionState.Open)
            {
                this._mysqlConnection.Close();
            }
        }
        public void Dispose()
        {
            if (this._mysqlConnection.State == ConnectionState.Open)
            {
                this._mysqlConnection.Close();
            }
            this._mysqlConnection.Dispose();
        }
        public void connect()
        {
            this.Open();
        }
        public void disconnect()
        {
            this.Close();
        }
        public IQueryAdapter getQueryReactor()
        {
            return this._adapter;
        }
        public bool isAvailable()
        {
            return false;
        }
        public void prepare()
        {
        }
        public void reportDone()
        {
            this.Dispose();
        }
        public MySqlCommand createNewCommand()
        {
            return this._mysqlConnection.CreateCommand();
        }
        public MySqlTransaction getTransaction()
        {
            return this._mysqlConnection.BeginTransaction();
        }
    }
}
