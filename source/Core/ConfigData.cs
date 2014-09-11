using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
namespace Cyber.Core
{
	internal class ConfigData
	{
		internal Dictionary<string, string> DBData;
		internal ConfigData(IQueryAdapter dbClient)
		{
			this.DBData = new Dictionary<string, string>();
			this.DBData.Clear();
			dbClient.setQuery("SELECT * FROM server_settings");
			DataTable table = dbClient.getTable();
			foreach (DataRow dataRow in table.Rows)
			{
				this.DBData.Add(dataRow[0].ToString(), dataRow[1].ToString());
			}
		}
	}
}
