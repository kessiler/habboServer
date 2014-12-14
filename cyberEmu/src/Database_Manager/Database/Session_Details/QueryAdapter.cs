namespace Database_Manager.Database.Session_Details
{
    using ConsoleWriter;
    using Database_Manager.Database;
    using Database_Manager.Database.Session_Details.Interfaces;
    using Database_Manager.Session_Details.Interfaces;
    using MySql.Data.MySqlClient;
    using System;
    using System.Data;

    internal class QueryAdapter : IRegularQueryAdapter
    {
        protected IDatabaseClient client;
        protected MySqlCommand command;

        public QueryAdapter(IDatabaseClient Client)
        {
            this.client = Client;
        }

        public void addParameter(string name, byte[] data)
        {
            this.command.Parameters.Add(new MySqlParameter(name, MySqlDbType.Blob, data.Length));
        }

        public void addParameter(string parameterName, object val)
        {
            this.command.Parameters.AddWithValue(parameterName, val);
        }

        public bool findsResult()
        {
            if (!dbEnabled)
            {
                return false;
            }
            DateTime now = DateTime.Now;
            bool hasRows = false;
            try
            {
                using (MySqlDataReader reader = this.command.ExecuteReader())
                {
                    hasRows = reader.HasRows;
                }
            }
            catch (Exception exception)
            {
                Writer.LogQueryError(exception, this.command.CommandText);
            }
            return hasRows;
        }

        public int getInteger()
        {
            if (!dbEnabled)
            {
                return 0;
            }
            DateTime now = DateTime.Now;
            int result = 0;
            try
            {
                object obj2 = this.command.ExecuteScalar();
                if (obj2 != null)
                {
                    int.TryParse(obj2.ToString(), out result);
                }
            }
            catch (Exception exception)
            {
                Writer.LogQueryError(exception, this.command.CommandText);
            }
            return result;
        }

        public DataRow getRow()
        {
            if (!dbEnabled)
            {
                return null;
            }
            DateTime now = DateTime.Now;
            DataRow row = null;
            try
            {
                DataSet dataSet = new DataSet();
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(this.command))
                {
                    adapter.Fill(dataSet);
                }
                if ((dataSet.Tables.Count > 0) && (dataSet.Tables[0].Rows.Count == 1))
                {
                    row = dataSet.Tables[0].Rows[0];
                }
            }
            catch (Exception exception)
            {
                Writer.LogQueryError(exception, this.command.CommandText);
            }
            return row;
        }

        public string getString()
        {
            if (!dbEnabled)
            {
                return string.Empty;
            }
            DateTime now = DateTime.Now;
            string str = string.Empty;
            try
            {
                object obj2 = this.command.ExecuteScalar();
                if (obj2 != null)
                {
                    str = obj2.ToString();
                }
            }
            catch (Exception exception)
            {
                Writer.LogQueryError(exception, this.command.CommandText);
            }
            return str;
        }

        public DataTable getTable()
        {
            DateTime now = DateTime.Now;
            DataTable dataTable = new DataTable();
            if (dbEnabled)
            {
                try
                {
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(this.command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
                catch (Exception exception)
                {
                    Writer.LogQueryError(exception, this.command.CommandText);
                }
            }
            return dataTable;
        }

        public long insertQuery()
        {
            if (!dbEnabled)
            {
                return 0L;
            }
            DateTime now = DateTime.Now;
            long lastInsertedId = 0L;
            try
            {
                this.command.ExecuteScalar();
                lastInsertedId = this.command.LastInsertedId;
            }
            catch (Exception exception)
            {
                Writer.LogQueryError(exception, this.command.CommandText);
            }
            return lastInsertedId;
        }

        public void runFastQuery(string query)
        {
            if (dbEnabled)
            {
                DateTime now = DateTime.Now;
                this.setQuery(query);
                this.runQuery();
            }
        }

        public void runQuery()
        {
            if (dbEnabled)
            {
                DateTime now = DateTime.Now;
                try
                {
                    this.command.ExecuteNonQuery();
                }
                catch (Exception exception)
                {
                    Writer.LogQueryError(exception, this.command.CommandText);
                }
            }
        }

        public void setQuery(string query)
        {
            this.command.Parameters.Clear();
            this.command.CommandText = query;
        }

        private static bool dbEnabled
        {
            get
            {
                return DatabaseManager.dbEnabled;
            }
        }
    }
}

