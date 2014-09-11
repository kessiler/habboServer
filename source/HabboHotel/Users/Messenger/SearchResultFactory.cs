using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
namespace Cyber.HabboHotel.Users.Messenger
{
	internal class SearchResultFactory
	{
		internal static List<SearchResult> GetSearchResult(string query)
		{
			List<SearchResult> list = new List<SearchResult>();
			DataTable table;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT id,username,motto,look,last_online FROM users WHERE username LIKE @query LIMIT 50");
				queryreactor.addParameter("query", query + "%");
				table = queryreactor.getTable();
			}
			foreach (DataRow dataRow in table.Rows)
			{
				uint userID = Convert.ToUInt32(dataRow[0]);
				string username = (string)dataRow[1];
				string motto = (string)dataRow[2];
				string look = (string)dataRow[3];
				string last_online = dataRow[4].ToString();
				SearchResult item = new SearchResult(userID, username, motto, look, last_online);
				list.Add(item);
			}
			return list;
		}
	}
}
