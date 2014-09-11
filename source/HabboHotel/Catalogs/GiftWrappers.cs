using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Core;
using System;
using System.Collections.Generic;
using System.Data;
namespace Cyber.HabboHotel.Catalogs
{
	internal class GiftWrappers
	{
		private List<uint> GiftWrappersList = new List<uint>();
		public List<uint> GetGiftWrappersList
		{
			get
			{
				return this.GiftWrappersList;
			}
		}
		public GiftWrappers(IQueryAdapter dbClient)
		{
			dbClient.setQuery("SELECT * FROM gift_wrappers");
			DataTable table = dbClient.getTable();
			if (table.Rows.Count <= 0)
			{
				Logging.LogCriticalException("Failed to load the Gift Wrappers");
				return;
			}
			foreach (DataRow dataRow in table.Rows)
			{
				this.GiftWrappersList.Add((uint)dataRow["baseid"]);
			}
		}
	}
}
