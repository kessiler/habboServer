using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Core;
using System;
using System.Collections.Generic;
using System.Data;
namespace Cyber.HabboHotel.Catalogs
{
	internal class GiftWrappers
	{
        internal List<uint> GiftWrappersList    = new List<uint>();
        internal List<uint> OldGiftWrappersList = new List<uint>();

        public GiftWrappers(IQueryAdapter dbClient)
		{
			dbClient.setQuery("SELECT * FROM gift_wrappers");
			DataTable table = dbClient.getTable();
            if (table.Rows.Count > 0)
            {
                foreach (DataRow dataRow in table.Rows)
                {
                    if (dataRow["type"].ToString() == "new")
                    {
                        this.GiftWrappersList.Add((uint)dataRow["sprite_id"]);
                    }
                    else if (dataRow["type"].ToString() == "old")
                    {
                        this.OldGiftWrappersList.Add((uint)dataRow["sprite_id"]);
                    }
                }
            }
		}
	}
}
