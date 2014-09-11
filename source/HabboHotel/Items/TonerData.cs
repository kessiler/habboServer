using Database_Manager.Database.Session_Details.Interfaces;
using Cyber.Messages;
using System;
using System.Data;
namespace Cyber.HabboHotel.Items
{
	internal class TonerData
	{
		internal int Enabled;
		internal uint ItemId;
		internal int Data1;
		internal int Data2;
		internal int Data3;
		internal TonerData(uint Item)
		{
			this.ItemId = Item;
			DataRow row;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT enabled,data1,data2,data3 FROM room_items_toner WHERE id=" + this.ItemId + " LIMIT 1");
				row = queryreactor.getRow();
			}
			if (row == null)
			{
				throw new NullReferenceException("No toner data found in the database for " + this.ItemId);
			}
			this.Enabled = int.Parse(row[0].ToString());
			this.Data1 = (int)row[1];
			this.Data2 = (int)row[2];
			this.Data3 = (int)row[3];
		}
		internal ServerMessage GenerateExtraData(ServerMessage Message)
		{
			Message.AppendInt32(0);
			Message.AppendInt32(5);
			Message.AppendInt32(4);
			Message.AppendInt32(this.Enabled);
			Message.AppendInt32(this.Data1);
			Message.AppendInt32(this.Data2);
			Message.AppendInt32(this.Data3);
			return Message;
		}
	}
}
