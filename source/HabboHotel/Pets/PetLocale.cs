using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
namespace Cyber.HabboHotel.Pets
{
	internal class PetLocale
	{
		private static Dictionary<string, string[]> values;
		internal static void Init(IQueryAdapter dbClient)
		{
			dbClient.setQuery("SELECT * FROM bots_pet_responses");
			DataTable table = dbClient.getTable();
			PetLocale.values = new Dictionary<string, string[]>();
			foreach (DataRow dataRow in table.Rows)
			{
				PetLocale.values.Add(dataRow[0].ToString(), dataRow[1].ToString().Split(new char[]
				{
					';'
				}));
			}
		}
		internal static string[] GetValue(string key)
		{
			string[] result;
			if (PetLocale.values.TryGetValue(key, out result))
			{
				return result;
			}
			return new string[]
			{
				key
			};
		}
	}
}
