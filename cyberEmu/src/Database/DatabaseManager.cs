using Database_Manager.Database.Session_Details.Interfaces;

namespace Cyber.Database
{
    internal sealed class DatabaseManager
    {
        private readonly string _connectionStr;

        public DatabaseManager(string ConnectionStr)
        {
            this._connectionStr = ConnectionStr;
        }
        public IQueryAdapter getQueryReactor()
        {
            IDatabaseClient databaseClient = new DatabaseConnection(this._connectionStr);
            databaseClient.connect();
            databaseClient.prepare();
            return databaseClient.getQueryReactor();

        }
        public void Destroy(){}
    }
}
