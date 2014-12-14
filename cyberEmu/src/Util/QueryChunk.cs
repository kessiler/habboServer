using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
namespace Cyber.Util
{
	internal class QueryChunk
	{
		private Dictionary<string, object> parameters;
		private StringBuilder queries;
		private int queryCount;
		private EndingType endingType;
		public QueryChunk()
		{
			this.parameters = new Dictionary<string, object>();
			this.queries = new StringBuilder();
			this.queryCount = 0;
			this.endingType = EndingType.Sequential;
		}
		public QueryChunk(string startQuery)
		{
			this.parameters = new Dictionary<string, object>();
			this.queries = new StringBuilder(startQuery);
			this.endingType = EndingType.Continuous;
			this.queryCount = 0;
		}
		internal void AddQuery(string query)
		{
			checked
			{
				this.queryCount++;
				this.queries.Append(query);
				switch (this.endingType)
				{
				case EndingType.Sequential:
					this.queries.Append(";");
					return;
				case EndingType.Continuous:
					this.queries.Append(",");
					return;
				default:
					return;
				}
			}
		}
		internal void AddParameter(string parameterName, object value)
		{
			this.parameters.Add(parameterName, value);
		}
		internal void Execute(IQueryAdapter dbClient)
		{
			if (this.queryCount == 0)
			{
				return;
			}
			this.queries = this.queries.Remove(checked(this.queries.Length - 1), 1);
			dbClient.setQuery(this.queries.ToString());
			foreach (KeyValuePair<string, object> current in this.parameters)
			{
				dbClient.addParameter(current.Key, current.Value);
			}
			dbClient.runQuery();
		}
		internal void Dispose()
		{
			this.parameters.Clear();
			this.queries.Clear();
			this.parameters = null;
			this.queries = null;
		}
	}
}
