using Database_Manager.Database.Session_Details.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
namespace Cyber.HabboHotel.RoomBots
{
	internal class BotManager
	{
		private List<RoomBot> Bots;
		internal BotManager()
		{
			this.Bots = new List<RoomBot>();
		}
		internal List<RoomBot> GetBotsForRoom(uint RoomId)
		{
			return new List<RoomBot>(
				from p in this.Bots
				where p.RoomId == RoomId
				select p);
		}
		internal static RoomBot GenerateBotFromRow(DataRow Row)
		{
			uint num = Convert.ToUInt32(Row["id"]);
			if (Row == null)
			{
				return null;
			}
			List<RandomSpeech> list = new List<RandomSpeech>();
			DataTable table;
			using (IQueryAdapter queryreactor = CyberEnvironment.GetDatabaseManager().getQueryReactor())
			{
				queryreactor.setQuery("SELECT text, shout FROM bots_speech WHERE bot_id = @id;");
				queryreactor.addParameter("id", num);
				table = queryreactor.getTable();
			}
			foreach (DataRow dataRow in table.Rows)
			{
				list.Add(new RandomSpeech((string)dataRow["text"], CyberEnvironment.EnumToBool(dataRow["shout"].ToString())));
			}
			List<BotResponse> list2 = new List<BotResponse>();
			return new RoomBot(num, Convert.ToUInt32(Row["user_id"]), Convert.ToUInt32(Row["room_id"]), AIType.Generic, "freeroam", (string)Row["name"], (string)Row["motto"], (string)Row["look"], int.Parse(Row["x"].ToString()), int.Parse(Row["y"].ToString()), (double)int.Parse(Row["z"].ToString()), 4, 0, 0, 0, 0, ref list, ref list2, (string)Row["gender"], (int)Row["dance"], Row["is_bartender"].ToString() == "1");
		}
		internal RoomBot GetBot(uint BotId)
		{
			return (
				from p in this.Bots
				where p.BotId == BotId
				select p).FirstOrDefault<RoomBot>();
		}
	}
}
