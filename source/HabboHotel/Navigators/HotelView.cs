using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Messages;
using System;
using System.Collections.Generic;
using System.Data;
namespace Cyber.HabboHotel.Navigators
{
	public class HotelView
	{
		internal List<SmallPromo> HotelViewPromosIndexers = new List<SmallPromo>();
		internal string FurniRewardName;
		internal int FurniRewardId;
		public HotelView()
		{
			this.list();
			this.loadReward();
		}
		private void loadReward()
		{
			DataRow row;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT hotelview_rewardpromo.furni_id, hotelview_rewardpromo.furni_name FROM hotelview_rewardpromo WHERE hotelview_rewardpromo.enabled = 1 LIMIT 1");
				row = queryreactor.getRow();
			}
			if (row != null)
			{
				this.FurniRewardId = Convert.ToInt32(row[0]);
				this.FurniRewardName = Convert.ToString(row[1]);
			}
		}
		private void list()
		{
			DataTable table;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT * from hotelview_promos WHERE hotelview_promos.enabled = '1' ORDER BY hotelview_promos.`index` DESC");
				table = queryreactor.getTable();
			}
			foreach (DataRow dataRow in table.Rows)
			{
				this.HotelViewPromosIndexers.Add(new SmallPromo(Convert.ToInt32(dataRow[0]), (string)dataRow[1], (string)dataRow[2], (string)dataRow[3], Convert.ToInt32(dataRow[4]), (string)dataRow[5], (string)dataRow[6]));
			}
		}
		public void RefreshPromoList()
		{
			this.HotelViewPromosIndexers.Clear();
			this.list();
		}
		internal ServerMessage SmallPromoComposer(ServerMessage Message)
		{
			Message.AppendInt32(this.HotelViewPromosIndexers.Count);
			foreach (SmallPromo current in this.HotelViewPromosIndexers)
			{
				current.Serialize(Message);
			}
			return Message;
		}
		public static SmallPromo load(int index)
		{
			DataRow row;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT hotelview_promos.`index`,hotelview_promos.header,hotelview_promos.body,hotelview_promos.button,hotelview_promos.in_game_promo,hotelview_promos.special_action,hotelview_promos.image,hotelview_promos.enabled FROM hotelview_promos WHERE hotelview_promos.`index` = @x LIMIT 1");
				queryreactor.addParameter("x", index);
				row = queryreactor.getRow();
			}
			return new SmallPromo(index, (string)row[1], (string)row[2], (string)row[3], Convert.ToInt32(row[4]), (string)row[5], (string)row[6]);
		}
	}
}
