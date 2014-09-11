using Database_Manager.Database.Session_Details.Interfaces;

namespace Cyber.Database
{
    internal sealed class DatabaseManager
    {
        private readonly string _connectionStr;
        private IDatabaseClient databaseClient;

        public DatabaseManager(string ConnectionStr)
        {
            this._connectionStr = ConnectionStr;
            this.databaseClient = new DatabaseConnection(this._connectionStr);
        }
        public IQueryAdapter getQueryReactor()
        {
            this.databaseClient.connect();
            return this.databaseClient.getQueryReactor();
        }
        public void Destroy()
        {
            this.databaseClient.disconnect();
        }
    }
}
