using System;
namespace Database_Manager.Database.Database_Exceptions
{
	public class QueryException : Exception
	{
		private string query;
		public QueryException(string message, string query) : base(message)
		{
			this.query = query;
		}
		public string getQuery()
		{
			return this.query;
		}
	}
}
