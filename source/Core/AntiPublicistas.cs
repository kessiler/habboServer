using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cyber.Core
{
	internal class AntiPublicistas
	{
        internal static Regex reg = new Regex("[^a-zA-Z0-9 ]");

        internal static string[] bannedHotels;

		internal static void Load(IQueryAdapter DBClient)
		{
            DBClient.setQuery("SELECT * FROM mercury_bannedhotels");
			DataTable Table = DBClient.getTable();
            bannedHotels = new string[Table.Rows.Count];

            int i = 0;
			foreach (DataRow dataRow in DBClient.getTable().Rows)
			{
				AntiPublicistas.bannedHotels[i] = (dataRow[0].ToString());
                i++;
			}
		}

		internal static bool CheckPublicistas(string Message)
		{
			if (string.IsNullOrWhiteSpace(Message))
			{
				return false;
			}
            // Quitar espacios y caracteres extraños.
            Message = Message.ToLower().Replace(" ", "").Replace("\u00a0", "").Replace("/", "").Replace("-", "").Replace("_", "").Replace("*", "").Replace("0", "o").Replace("4", "a").Replace("1", "i");


            return bannedHotels.Any(Message.Contains) && !Message.Contains("ban") && !Message.Contains("mip");
		}
	}
}
